using Pets_identifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Pets_identifier
{
	public partial class AzureTable : ContentPage
	{
		public AzureTable()
		{
			InitializeComponent();
		}

		async void Handle_ClickedAsync(object sender, System.EventArgs e)
		{
			string pet = AzureManager.AzureManagerInstance.GetPet();
			if (pet != null)
			{
				List<PetIdentifier> petInfo = await AzureManager.AzureManagerInstance.GetPetInformation();
				PetList.ItemsSource = petInfo.Where(p => String.Equals(p.Pet, pet));
			}
		}
	}
}