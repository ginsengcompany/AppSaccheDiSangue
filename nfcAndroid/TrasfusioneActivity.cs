using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using nfcAndroid.Classes;
using nfcAndroid.Service;

namespace nfcAndroid
{
    [Activity(Label = "TrasfusioneActivity")]
    public class TrasfusioneActivity : Activity
    {
        private NfcAdapter m_Adapter = null; //Adapter per il servizio di lettura e scrittura del Tag NFC
        private TextView nomePaziente, cognomePaziente, gruppoPaziente, rhPaziente, gruppoSacca, rhSacca, statoSacca;
        private bool scanPaziente, scanSacca;
        private Sacca sacca;
        private Paziente paziente;
        private Button btnStartScanPaziente, btnStartScanSacca, btnStopScanPaziente, btnStopScanSacca, btnAssegnaTrasfusione;
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

        private void inizializzaOggetti()
        {
            nomePaziente = FindViewById<TextView>(Resource.Id.nomePaziente);
            cognomePaziente = FindViewById<TextView>(Resource.Id.cognomePaziente);
            gruppoPaziente = FindViewById<TextView>(Resource.Id.gruppoPaziente);
            rhPaziente = FindViewById<TextView>(Resource.Id.rhPaziente);
            gruppoSacca = FindViewById<TextView>(Resource.Id.gruppoSacca);
            rhSacca = FindViewById<TextView>(Resource.Id.rhSacca);
            statoSacca = FindViewById<TextView>(Resource.Id.statoSaccaTrasfusione);
            btnStartScanPaziente = FindViewById<Button>(Resource.Id.startScanPaziente);
            btnStartScanSacca = FindViewById<Button>(Resource.Id.startScanSacca);
            btnStopScanPaziente = FindViewById<Button>(Resource.Id.stopScanPaziente);
            btnStopScanSacca = FindViewById<Button>(Resource.Id.stopScanSacca);
            btnAssegnaTrasfusione = FindViewById<Button>(Resource.Id.assegnaTrasfusione);
            btnStartScanPaziente.Click += (Object sender, EventArgs eventArgs) =>
            {
                EnableScanPaziente();
            };
            btnStartScanSacca.Click += (Object sender, EventArgs eventArgs) =>
                {
                    EnableScanSacca();
                };
            btnStopScanPaziente.Click += (Object sender, EventArgs eventArgs) =>
            {
                DisableScanPaziente();
            };
            btnStopScanSacca.Click += (Object sender, EventArgs eventArgs) =>
            {
                DisableScanSacca();
            };
            btnAssegnaTrasfusione.Click += async (Object sender, EventArgs eventArgs) =>
            {
                await inviaDatiTrasfusione();
            };
            btnStopScanPaziente.Enabled = false;
            btnStopScanSacca.Enabled = false;
        }

        private async Task inviaDatiTrasfusione()
        {
            if (!sacca.disponibile)
                Toast.MakeText(this, "La sacca non è disponibile per l'assegnazione", ToastLength.Short).Show();
            else if ((paziente != null && !string.IsNullOrEmpty(paziente.uid.Trim())) && (sacca != null && !string.IsNullOrEmpty(sacca.uid.Trim())))
            {
                REST<Paziente, string> rest = new REST<Paziente, string>();
                paziente.uidSacca = sacca.uid;
                var response = await rest.PostJson("http://192.168.125.14:3000/pazienti/assegnatrasfusione", paziente);
                if (rest.responseMessage == System.Net.HttpStatusCode.OK)
                {
                    nomePaziente.Text = "";
                    cognomePaziente.Text = "";
                    gruppoPaziente.Text = "";
                    rhPaziente.Text = "";
                    gruppoSacca.Text = "";
                    rhSacca.Text = "";
                    statoSacca.Text = "";
                    sacca = new Sacca();
                    paziente = new Paziente();
                }
                Toast.MakeText(this, rest.warning, ToastLength.Short).Show();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Trasfusione);
            inizializzaOggetti();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (m_Adapter != null)
                m_Adapter.DisableForegroundDispatch(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (scanPaziente)
                EnableScanPaziente();
            else if (scanSacca)
                EnableScanSacca();
        }

        //Converte un array di byte in string in formato esadecimale, utilizzato per convertire l'UID del tag rilevato
        private string bin2hex(byte[] data)
        {
            var hex = new StringBuilder(16);
            foreach (byte a in data)
                hex.Append(string.Format("{0:X2}", a));
            return hex.ToString();
        }

        private void EnableScanPaziente()
        {
            try
            {
                //Inizializza l'intent filter indicando che avvia un'azione di intent quando trova un tag
                IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                var filters = new[] { tagDetected };
                // Quando un tag NFC viene rilevato, android utilizza il PendingIntent per tornare all'activity corrente.
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                Adapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
                scanPaziente = true;
                btnStartScanPaziente.Enabled = !scanPaziente;
                btnStopScanPaziente.Enabled = scanPaziente;
                btnStartScanSacca.Enabled = !scanPaziente;
                btnStopScanSacca.Enabled = !scanPaziente;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Exception", ToastLength.Short).Show();
                throw;
            }
        }

        private void DisableScanPaziente()
        {
            if (m_Adapter != null)
            {
                m_Adapter.DisableForegroundDispatch(this);
                scanPaziente = false;
                btnStartScanPaziente.Enabled = !scanPaziente;
                btnStopScanPaziente.Enabled = scanPaziente;
                btnStartScanSacca.Enabled = !scanPaziente;
                btnStopScanSacca.Enabled = scanPaziente;
            }
        }

        private void EnableScanSacca()
        {
            try
            {
                //Inizializza l'intent filter indicando che avvia un'azione di intent quando trova un tag
                IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                var filters = new[] { tagDetected };
                // Quando un tag NFC viene rilevato, android utilizza il PendingIntent per tornare all'activity corrente.
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                Adapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
                scanSacca = true;
                btnStartScanPaziente.Enabled = !scanSacca;
                btnStopScanPaziente.Enabled = !scanSacca;
                btnStartScanSacca.Enabled = !scanSacca;
                btnStopScanSacca.Enabled = scanSacca;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                throw;
            }
        }

        private void DisableScanSacca()
        {
            if (m_Adapter != null)
            {
                m_Adapter.DisableForegroundDispatch(this);
                scanSacca = false;
                btnStartScanPaziente.Enabled = !scanSacca;
                btnStopScanPaziente.Enabled = scanSacca;
                btnStartScanSacca.Enabled = !scanSacca;
                btnStopScanSacca.Enabled = scanSacca;
            }
        }

        // Il metodo OnNewIntent viene invocato dal sistema android.
        protected override async void OnNewIntent(Intent intent)
        {
            if (scanPaziente) //L'operatore ha cliccato sul button per lo scan del tag relativo al paziente
            {
                try
                {
                    Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag; //Cast dei dati presenti nell'intent nella classe Tag (classe di tag generica)
                    if (tag == null)
                    {
                        return;
                    }
                    byte[] Uid = tag.GetId(); //Recupera l'UID del tag rilevato
                    string UidString = bin2hex(Uid); //Converte l'UID in string formato esadecimale
                    paziente = new Paziente();
                    paziente.uid = UidString;
                    if(await paziente.getPaziente(this))
                    {
                        nomePaziente.Text = paziente.nome;
                        cognomePaziente.Text = paziente.cognome;
                        gruppoPaziente.Text = paziente.gruppo;
                        rhPaziente.Text = paziente.rh;
                    }
                    else
                    {
                        nomePaziente.Text = "";
                        cognomePaziente.Text = "";
                        gruppoPaziente.Text = "";
                        rhPaziente.Text = "";
                    }
                    /*
                    MifareClassic mifareTag = MifareClassic.Get(tag); //Cast del Tag rilevato con la classe MifareClassic
                    mifareTag.Connect();//Abilita le funzioni di Input Output con il tag
                    if (mifareTag.AuthenticateSectorWithKeyB(3, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF })) //Effettua l'autenticazione sul settore 3 con la chiave B
                    {
                        int firstBlock = mifareTag.SectorToBlock(3);//Ritorna il primo blocco del settore 3
                        var read = mifareTag.ReadBlock(firstBlock);//Legge sul blocco firstBlock (la lettura avviene in byte)
                                                                   //Converte i byte in string formato UTF-32
                        var str1 = new StringBuilder(2);
                        str1.Append(char.ConvertFromUtf32(read[0]));
                        LogMessage.Text = str1.ToString(); //Gruppo sanguigno
                        sacca = new Sacca();

                        sacca.uid = UidString;
                        sacca.gruppo = str1.ToString().Trim();
                        await sacca.insertSacca(this);
                        //Scrive sul blocco firstBlock l'array di byte (l'array deve essere di 16 byte)
                        mifareTag.WriteBlock(firstBlock, new byte[] { 66, 0x00, 49, 0x00, 0x00, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 });
                        Toast.MakeText(this, "Write succesfully", ToastLength.Short).Show();

                    }
                    */
                }
                catch (TagLostException e) //Cattura l'eccezione nel caso in cui il tag viene allontanato dal dispositivo android
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                }
            }
            else if (scanSacca) //L'operatore ha cliccato sul button per lo scan del tag relativo alla sacca di sangue
            {
                try
                {
                    Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag; //Cast dei dati presenti nell'intent nella classe Tag (classe di tag generica)
                    if (tag == null)
                    {
                        return;
                    }
                    sacca = new Sacca();
                    byte[] Uid = tag.GetId(); //Recupera l'UID del tag rilevato
                    string UidString = bin2hex(Uid); //Converte l'UID in string formato esadecimale
                    sacca.uid = UidString;
                    if (await sacca.getSacca(this))
                    {
                        gruppoSacca.Text = sacca.gruppo;
                        rhSacca.Text = sacca.rh;
                        statoSacca.Text = sacca.stato;
                    }
                    else
                    {
                        gruppoSacca.Text = "";
                        rhSacca.Text = "";
                        statoSacca.Text = "";
                    }
                    /*
                    MifareClassic mifareTag = MifareClassic.Get(tag); //Cast del Tag rilevato con la classe MifareClassic
                    mifareTag.Connect();//Abilita le funzioni di Input Output con il tag
                    if (mifareTag.AuthenticateSectorWithKeyB(3, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF })) //Effettua l'autenticazione sul settore 3 con la chiave B
                    {
                        int firstBlock = mifareTag.SectorToBlock(3);//Ritorna il primo blocco del settore 3
                        var read = mifareTag.ReadBlock(firstBlock);//Legge sul blocco firstBlock (la lettura avviene in byte)
                                                                   //Converte i byte in string formato UTF-32
                        var str1 = new StringBuilder(2);
                        str1.Append(char.ConvertFromUtf32(read[0]));
                        LogMessage.Text = str1.ToString(); //Gruppo sanguigno
                        sacca = new Sacca();

                        sacca.uid = UidString;
                        sacca.gruppo = str1.ToString().Trim();
                        await sacca.insertSacca(this);
                        //Scrive sul blocco firstBlock l'array di byte (l'array deve essere di 16 byte)
                        mifareTag.WriteBlock(firstBlock, new byte[] { 66, 0x00, 49, 0x00, 0x00, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00 });
                        Toast.MakeText(this, "Write succesfully", ToastLength.Short).Show();

                    }
                    */
                }
                catch (TagLostException e) //Cattura l'eccezione nel caso in cui il tag viene allontanato dal dispositivo android
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}