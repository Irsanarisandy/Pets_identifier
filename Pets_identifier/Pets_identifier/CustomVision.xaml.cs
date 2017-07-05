using Newtonsoft.Json;
using Pets_identifier.Model;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Pets_identifier
{
	public partial class CustomVision : ContentPage
	{
		private string result1;
		private string result2;

		public CustomVision()
		{
			InitializeComponent();
		}

		private async void LoadCamera(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", ":( No camera available.", "OK");
				return;
			}

			MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				PhotoSize = PhotoSize.Medium,
				Directory = "Sample",
				Name = $"{DateTime.UtcNow}.png"
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});

			await MakePredictionRequest(file);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

		async Task MakePredictionRequest(MediaFile file)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "69ac7f16e50a4c8fa1f8fa2f7e882d1e");

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/821dc763-ec21-420a-986e-30ccc435bda6/image?iterationId=8ce43913-33eb-46dd-9881-4c76168cc6bd";

			HttpResponseMessage response;

			byte[] byteData = GetImageAsByteArray(file);

			using (var content = new ByteArrayContent(byteData))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(url, content);

				if (response.IsSuccessStatusCode)
				{
					var responseString = await response.Content.ReadAsStringAsync();

					EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);

					double max = responseModel.Predictions.Max(m => m.Probability);

					if (max >= 0.5)
					{
						var results = responseModel.Predictions.OrderByDescending(p => p.Probability);
						result1 = results.Take(1).Single().Tag;
						result2 = results.Skip(1).Take(1).Single().Tag;
						string[] tempArray = { "Hamster", "Rabbit" };
						if ((String.Equals(result1, "Small Pet") && Array.IndexOf(tempArray, result2) > -1) || (String.Equals(result2, "Small Pet") && Array.IndexOf(tempArray, result1) > -1))
						{
							AzureManager.AzureManagerInstance.SetPet("Small Pet");
							PetShopLink.Text = "http://www.animates.co.nz/small-pet";
						}
						else
						{
							string temp = (String.Equals(result1, "Small Pet")) ? result2 : result1;
							AzureManager.AzureManagerInstance.SetPet(temp);
							PetShopLink.Text = "http://www.animates.co.nz/" + temp.ToLower();
						}
						TagLabel.Text = (String.Equals(result1, "Small Pet")) ? result2 : result1;
						TagLabel.Text += ": ";
						PetShopLink.GestureRecognizers.Add(new TapGestureRecognizer
						{
							Command = new Command(() => {
								Device.OpenUri(new Uri(PetShopLink.Text));
							})
						});
					}
					else
					{
						TagLabel.Text = "Not Pet";
						PetShopLink.Text = "";
					}
				}
				
				file.Dispose();
			}
		}
	}
}