using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Pets_identifier
{
	public partial class AzureTable : ContentPage
	{
		public AzureTable()
		{
			InitializeComponent();
		}

		private async void GetItemCatAndLink(object sender, EventArgs e)
		{
			string pet = AzureManager.AzureManagerInstance.GetPet();
			if (pet != null)
			{
				List<PetIdentifier> petInfo = await AzureManager.AzureManagerInstance.GetPetInformation();
				PetList.ItemsSource = petInfo.Where(p => String.Equals(p.Pet, pet));
			}
		}

		private void PetItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null)
			{
				return;
			}
			((ListView)sender).SelectedItem = null;
			Device.OpenUri(new Uri(((PetIdentifier)e.SelectedItem).Link.ToString()));
		}
	}
}