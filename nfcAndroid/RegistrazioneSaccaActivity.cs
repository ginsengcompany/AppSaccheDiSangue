using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using nfcAndroid.Classes;

namespace nfcAndroid
{
    [Activity(Label = "RegistrazioneSaccaActivity")]
    public class RegistrazioneSaccaActivity : Activity
    {

        private TextView textIdSacca;
        private Button btnRegistraSacca;
        private bool _inReadMode;
        private Sacca sacca;
        private NfcAdapter m_Adapter = null; //Adapter per il servizio di lettura e scrittura del Tag NFC

        private NfcAdapter Adapter //Proprietà relativa alla variabile m_adapter
        {
            get
            {
                if (m_Adapter == null)
                {
                    m_Adapter = NfcAdapter.GetDefaultAdapter(this); //Assegna L'adapter di default
                }
                return m_Adapter; //Ritorna l'adapter
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RegistrazioneSacca);
            initView();
            // Create your application here
        }

        private void initView()
        {
            textIdSacca = FindViewById<TextView>(Resource.Id.uidSaccaRegistrazione);
            btnRegistraSacca = FindViewById<Button>(Resource.Id.btnregistraSacca);
            if (Adapter == null)
            {
                Toast.MakeText(this, "NFC non supportato", ToastLength.Short).Show();
                btnRegistraSacca.Enabled = false;
            }
            else
                btnRegistraSacca.Click += async (object sender, EventArgs args) =>
                {
                    if(sacca != null)
                        await sacca.insertSacca(this);
                };
        }

        //Converte un array di byte in string in formato esadecimale, utilizzato per convertire l'UID del tag rilevato
        private string bin2hex(byte[] data)
        {
            var hex = new StringBuilder(16);
            foreach (byte a in data)
                hex.Append(string.Format("{0:X2}", a));
            return hex.ToString();
        }

        protected override void OnResume()
        {
            base.OnResume();
            EnableReadMode();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (m_Adapter != null)
                m_Adapter.DisableForegroundDispatch(this);
        }

        private void EnableReadMode()
        {
            try
            {
                _inReadMode = true;
                //Inizializza l'intent filter indicando che avvia un'azione di intent quando trova un tag
                IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                var filters = new[] { tagDetected };
                // Quando un tag NFC viene rilevato, android utilizza il PendingIntent per tornare all'activity corrente.
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                Adapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                throw;
            }
        }

        // Il metodo OnNewIntent viene invocato dal sistema android.
        protected override async void OnNewIntent(Intent intent)
        {
            if (_inReadMode)
            {
                try
                {
                    _inReadMode = false;
                    sacca = new Sacca();
                    Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag; //Cast dei dati presenti nell'intent nella classe Tag (classe di tag generica)
                    if (tag == null)
                    {
                        return;
                    }
                    byte[] Uid = tag.GetId(); //Recupera l'UID del tag rilevato
                    string UidString = bin2hex(Uid); //Converte l'UID in string formato esadecimale
                    sacca.uid = UidString;
                    textIdSacca.Text = UidString;
                        /*
                        mifareTag.WriteBlock(firstBlock, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                        mifareTag.WriteBlock(firstBlock + 1, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                        Toast.MakeText(this, "Write succesfully", ToastLength.Short).Show();
                        */
                }
                catch (Exception e) //Cattura l'eccezione nel caso in cui il tag viene allontanato dal dispositivo android
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}