using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pets_identifier
{
	public class AzureManager
	{
		private static AzureManager instance;
		private MobileServiceClient client;
		private IMobileServiceTable<PetIdentifier> petTable;
		private string pet;

		private AzureManager()
		{
			client = new MobileServiceClient("http://pet-identifier.azurewebsites.net/");
			petTable = client.GetTable<PetIdentifier>();
		}

		public MobileServiceClient AzureClient
		{
			get { return client; }
		}

		public static AzureManager AzureManagerInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new AzureManager();
				}

				return instance;
			}
		}

		public void SetPet(string pet)
		{
			this.pet = pet;
		}

		public string GetPet()
		{
			return pet;
		}

		public Task<List<PetIdentifier>> GetPetInformation()
		{
			return petTable.ToListAsync();
		}
	}
}
