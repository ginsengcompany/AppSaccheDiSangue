using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Operatore
    {
        public string uid { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string tipoOperatore { get; set; }
        public int codice_operatore { get; set; }

        public async Task<bool> getOperatore(Context context)
        {
            REST<object, Operatore> rest = new REST<object, Operatore>();
            List<Header> headers = new List<Header>();
            headers.Add(new Header("uid", this.uid));
            var response = await rest.GetSingleJson("http://192.168.125.14:3000/operatori/take", headers);
            if (rest.responseMessage == System.Net.HttpStatusCode.OK)
            {
                this.nome = response.nome;
                this.cognome = response.cognome;
                this.tipoOperatore = response.tipoOperatore;
                this.codice_operatore = response.codice_operatore;
                Toast.MakeText(context, "Login effettuata con successo", ToastLength.Short).Show();
                return true;
            }
            else
            {
                Toast.MakeText(context, rest.warning, ToastLength.Short).Show();
                return false;
            }
                
        }
    }
}