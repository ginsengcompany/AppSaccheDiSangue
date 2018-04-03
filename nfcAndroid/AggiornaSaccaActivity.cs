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
    [Activity(Label = "AggiornaSaccaActivity")]
    public class AggiornaSaccaActivity : Activity
    {
        private string[] gruppi = new string[] { "0", "A", "B", "AB" };
        private string[] rh = new string[] { "Positivo", "Negativo" };
        private TextView idSacca, statoSacca;
        private EditText editGruppo, editRh;
        private bool _inReadMode;
        private Button btnUpdateSacca;
        private Sacca sacca;
        private Dialog dialogGruppo, dialogRh;
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

        private void initView()
        {
            idSacca = FindViewById<TextView>(Resource.Id.uidSaccaUpdate);
            statoSacca = FindViewById<TextView>(Resource.Id.textStato);
            editGruppo = FindViewById<EditText>(Resource.Id.editGruppoSacca);
            editRh = FindViewById<EditText>(Resource.Id.editRhSacca);
            btnUpdateSacca = FindViewById<Button>(Resource.Id.btnUpdateSacca);
            dialogGruppo = createDialogGruppo();
            dialogRh = createDialogRh();
            btnUpdateSacca.Click += async (object sender, EventArgs args) =>
            {
                if (!string.IsNullOrEmpty(editGruppo.Text.Trim()) && !string.IsNullOrEmpty(editRh.Text.Trim()))
                {
                    sacca.gruppo = editGruppo.Text;
                    sacca.rh = editRh.Text;
                    await sacca.updateSacca(this);
                }
                    
            };
            editGruppo.Click += (Object sender, EventArgs args) =>
            {
                dialogGruppo.Show();
            };
            editRh.Click += (Object sender, EventArgs args) =>
            {
                dialogRh.Show();
            };
            editGruppo.Focusable = false;
            editRh.Focusable = false;
            editGruppo.Text = "";
            editRh.Text = "";
            statoSacca.Text = "";
        }

        private Dialog createDialogGruppo()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Scegli il gruppo sanguigno")
                .SetItems(gruppi, (Object sender, DialogClickEventArgs args)=> {
                    if (args.Which > -1)
                        editGruppo.Text = gruppi[args.Which];
                });
            return builder.Create();
        }

        private Dialog createDialogRh()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Scegli il fattore Rhesus")
                .SetItems(rh, (Object sender, DialogClickEventArgs args) =>
                {
                    if (args.Which > -1)
                        editRh.Text = rh[args.Which];
                });
            return builder.Create();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AggiornaSacca);
            initView();
            // Create your application here
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
            EnableScanSacca();
        }

        private string bin2hex(byte[] data)
        {
            var hex = new StringBuilder(16);
            foreach (byte a in data)
                hex.Append(string.Format("{0:X2}", a));
            return hex.ToString();
        }

        private void EnableScanSacca()
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

        protected override async void OnNewIntent(Intent intent)
        {
            if (_inReadMode)
            {
                try
                {
                    _inReadMode = false;
                    Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag; //Cast dei dati presenti nell'intent nella classe Tag (classe di tag generica)
                    if (tag == null)
                    {
                        return;
                    }
                    byte[] Uid = tag.GetId(); //Recupera l'UID del tag rilevato
                    string UidString = bin2hex(Uid); //Converte l'UID in string formato esadecimale
                    sacca = new Sacca();
                    sacca.uid = UidString;
                    idSacca.Text = UidString;
                    if (await sacca.getSacca(this))
                    {
                        editGruppo.Text = sacca.gruppo;
                        editRh.Text = sacca.rh;
                        statoSacca.Text = sacca.stato;
                    }
                    else
                    {
                        editGruppo.Text = "";
                        editRh.Text = "";
                        statoSacca.Text = "";
                    }
                }
                catch (TagLostException e) //Cattura l'eccezione nel caso in cui il tag viene allontanato dal dispositivo android
                {
                    Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}