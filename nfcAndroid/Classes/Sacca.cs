using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using nfcAndroid.Service;

namespace nfcAndroid.Classes
{
    public class Sacca
    {
        public string uid { get; set; }
        public string gruppo { get; set; }
        public string rh { get; set; }
        public bool disponibile { get; set; }
        public string stato { get; set; }
        public int codice_stato { get; set; }

        public async Task insertSacca(Context context)
        {
            REST<Sacca,string> rest = new REST<Sacca, string>();
            var response = await rest.PostJson("http://192.168.125.14:3001/sacche/insert", this);
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            if (rest.responseMessage == HttpStatusCode.Created)
                alert.SetTitle("SUCCESS");
            else
                alert.SetTitle("ERROR");
            alert.SetMessage(rest.warning);
            alert.SetPositiveButton("OK",(senderAlert,args) =>
            {

            });
            alert.Show();
        }

        public async Task<bool> getSacca(Context context)
        {
            REST<object, Sacca> rest = new REST<object, Sacca>();
            List<Header> headers = new List<Header>();
            headers.Add(new Header("uid", this.uid));
            var response = await rest.GetSingleJson("http://192.168.125.14:3001/sacche/take", headers);
            if (rest.responseMessage != HttpStatusCode.OK)
            {
                Toast.MakeText(context, rest.warning, ToastLength.Short).Show();
                return false;
            }
            this.gruppo = response.gruppo;
            this.rh = response.rh;
            this.disponibile = response.disponibile;
            this.stato = response.stato;
            this.codice_stato = response.codice_stato;
            return true;
        }

        public async Task<bool> updateSacca(Context context)
        {
            REST<Sacca, string> rest = new REST<Sacca, string>();
            var response = await rest.PostJson("http://192.168.125.14:3001/sacche/update",this);
            
            if (rest.responseMessage == HttpStatusCode.OK)
            {
                Toast.MakeText(context, "La sacca è stat aggiornata correttamente", ToastLength.Short).Show();
                return true;
            }
            Toast.MakeText(context, response, ToastLength.Short).Show();
            return false;
        }
    }
}