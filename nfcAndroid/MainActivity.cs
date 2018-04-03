using Android.App;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Content;
using System;
using System.Text;
using Android.Nfc.Tech;
using Android.Util;
using nfcAndroid.Classes;
using Newtonsoft.Json;

namespace nfcAndroid
{
    [Activity(Label = "nfcAndroid", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button btnLogin;
        private NfcAdapter m_Adapter = null; //Adapter per il servizio di lettura e scrittura del Tag NFC
        private bool _inReadMode, scanLogin;
        private Operatore operatore;
        private Intent toActivity;
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
            SetContentView(Resource.Layout.Main);
            btnLogin = FindViewById<Button>(Resource.Id.buttonLogin);
            scanLogin = false;
            if (Adapter == null)
            {
                Toast.MakeText(this, "NFC non supportato", ToastLength.Short).Show();
                btnLogin.Enabled = false;
            }
            else
            {
                btnLogin.Click += (Object sender, EventArgs eventArgs) =>
                {
                    scanLogin = true;
                    EnableReadMode();
                };
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (scanLogin)
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

        //Converte un array di byte in string in formato esadecimale, utilizzato per convertire l'UID del tag rilevato
        private string bin2hex(byte[] data)
        {
            var hex = new StringBuilder(16);
            foreach (byte a in data)
                hex.Append(string.Format("{0:X2}", a));
            return hex.ToString();
        }

        // Il metodo OnNewIntent viene invocato dal sistema android.
        protected override async void OnNewIntent(Intent intent)
        {
            if (_inReadMode)
            {
                try
                {
                    _inReadMode = false;
                    if (scanLogin)
                    {
                        operatore = new Operatore();
                        Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag; //Cast dei dati presenti nell'intent nella classe Tag (classe di tag generica)
                        if (tag == null)
                        {
                            return;
                        }
                        byte[] Uid = tag.GetId(); //Recupera l'UID del tag rilevato
                        string UidString = bin2hex(Uid); //Converte l'UID in string formato esadecimale
                        operatore.uid = UidString;
                        if (await operatore.getOperatore(this))
                        {
                            scanLogin = false;
                            if (operatore.codice_operatore == 1)
                            {
                                toActivity = new Intent(this, typeof(TrasfusioneActivity));
                                toActivity.PutExtra("operatore", JsonConvert.SerializeObject(operatore));
                                StartActivity(toActivity);
                            }
                            else if (operatore.codice_operatore == 2)
                            {
                                toActivity = new Intent(this, typeof(RegistrazioneSaccaActivity));
                                toActivity.PutExtra("operatore", JsonConvert.SerializeObject(operatore));
                                StartActivity(toActivity);
                            }
                            else if (operatore.codice_operatore == 3)
                            {
                                toActivity = new Intent(this, typeof(AggiornaSaccaActivity));
                                toActivity.PutExtra("operatore", JsonConvert.SerializeObject(operatore));
                                StartActivity(toActivity);
                            }
                        }
                    }
                    else
                        Toast.MakeText(this, "Clicca sul pulsante per avviare lo scan", ToastLength.Short).Show();
                }
                catch (Exception e) //Cattura l'eccezione nel caso in cui il tag viene allontanato dal dispositivo android
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}

