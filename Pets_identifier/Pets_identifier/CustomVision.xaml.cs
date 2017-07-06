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

			var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				PhotoSize = PhotoSize.Medium,
				Directory = "Sample",
				Name = $"{DateTime.UtcNow}.png",
				AllowCropping = true
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});

			await MakePredictionRequest(file);
        }

		private async void LoadGallery(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
			{
				PhotoSize = PhotoSize.Medium
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});

			await MakePredictionRequest(file);
		}

        private static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            var binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

		private async Task MakePredictionRequest(MediaFile file)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "69ac7f16e50a4c8fa1f8fa2f7e882d1e");

			var url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/821dc763-ec21-420a-986e-30ccc435bda6/image?iterationId=8ce43913-33eb-46dd-9881-4c76168cc6bd";

			HttpResponseMessage response;

			var byteData = GetImageAsByteArray(file);

			using (var content = new ByteArrayContent(byteData))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(url, content);

				if (response.IsSuccessStatusCode)
				{
					var responseString = await response.Content.ReadAsStringAsync();
					var responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);
					var results = responseModel.Predictions.OrderByDescending(p => p.Probability);
					var result1 = results.Take(1).Single();
					var result2 = results.Skip(1).Take(1).Single();

					if (result1.Probability > 0.5)
					{
						string[] tempArray = { "Hamster", "Rabbit" };

						if ((String.Equals(result2.Tag, "Small Pet") && Array.IndexOf(tempArray, result1.Tag) > -1) || 
							(String.Equals(result1.Tag, "Small Pet") && Array.IndexOf(tempArray, result2.Tag) > -1 && result2.Probability > 0.5))
						{
							AzureManager.AzureManagerInstance.SetPet("Small Pet");
							TagLabel.Text = (String.Equals(result1.Tag, "Small Pet")) ? result2.Tag + ": " : result1.Tag + ": ";
							PetShopLink.Text = "http://www.animates.co.nz/small-pet";
						}

						else if (!String.Equals(result1.Tag, "Small Pet"))
						{
							AzureManager.AzureManagerInstance.SetPet(result1.Tag);
							TagLabel.Text = result1.Tag + ": ";
							PetShopLink.Text = "http://www.animates.co.nz/" + result1.Tag.ToLower();
						}

						else
						{
							TagLabel.Text = "Not Pet";
							PetShopLink.Text = "";
						}

						if (PetShopLink.Text != null && PetShopLink.Text != "")
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