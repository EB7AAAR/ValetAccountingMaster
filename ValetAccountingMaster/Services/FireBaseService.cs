using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Collections;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValetAccountingMaster.Model;

namespace ValetAccountingMaster.Services
{
    public class FireBaseService
    {
        FirebaseClient firebaseClient;
        public FireBaseService(List<Record> records)
        {
            this.Records = records;
            Connect();
        }

        public List<Record> Records { get; set; }



        async void Connect()
        {
            var auth = "VpHKztdyUC6A8omWOdUsvsBJGWOcNluSttprQrc6"; // your app secret
            firebaseClient = new FirebaseClient(
              "https://valet-9349d-default-rtdb.firebaseio.com/",
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });
        }

        public async Task GetRecords()
        {
            var recs = await firebaseClient
                .Child("Records")
                .OrderByKey()
                .OnceAsync<Record>();

            Records.Clear();
            foreach (var record in recs)
            {
                Records.Add(record.Object);
            }
        }

        public async Task GetPermanentRecords()
        {
            var recs = await firebaseClient
                .Child("PermanentRecords")
                .OrderByKey()
                .OnceAsync<Record>();

            Records.Clear();
            foreach (var record in recs)
            {
               Records.Add(record.Object);
            }
        }

        public async Task DeleteRecord(Record rec)
        {
            var x = (rec.Date.Year).ToString() +
                    "-" +
                    (rec.Date.Month).ToString() +
                    "-" +
                    (rec.Date.Day).ToString() +
                    "-" +
                    rec.ID;

            await firebaseClient
                .Child("Records")
                .Child(x)
                .DeleteAsync();
        }
        public async Task SaveMonthRecord(MonthRecord rec)
        {
            var x = (rec.Date.Year).ToString() +
                    "-" +
                    (rec.Date.Month).ToString() +
                    "-" +
                    (rec.Date.Day).ToString() +
                    "-" +
                    rec.ID;

            await firebaseClient
                .Child("MonthRecords")
                .Child(x)
                .PutAsync(rec);
        }
    }
}

