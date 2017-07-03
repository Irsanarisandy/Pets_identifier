using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/821dc763-ec21-420a-986e-30ccc435bda6/image?iterationId=6092ed69-5fb9-458b-b9e6-5c059404a273";

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

					TagLabel.Text = (max >= 0.5) ? "Pet" : "Not pet";

				}

				//Get rid of file once we have finished using it
				file.Dispose();
			}
		}
	}
}