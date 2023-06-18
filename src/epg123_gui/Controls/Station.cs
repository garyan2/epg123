using GaRyan2.SchedulesDirectAPI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace epg123
{
    internal class MemberStation
    {
        public override string ToString()
        {
            return $"{CallSign} - {Name} - ({StationId})";
        }

        public MemberStation(LineupStation station, SdChannelDownload options, CheckBox autoAdd)
        {
            if (options == null) IsNew = true;
            Station = station;
            StationOptions = options ?? new SdChannelDownload { StationId = Station.StationId, CallSign = Station.Callsign };

            // preset _include to avoid stack overflow
            if (IsNew && !autoAdd.Checked) StationOptions.StationId = "-" + StationId;
            _include = !StationOptions.StationId.StartsWith("-");

            autoAdd.CheckedChanged += (sender, args) =>
            {
                if (IsNew) Include = autoAdd.Checked;
            };
        }

        private bool IsAtsc
        {
            get
            {
                var names = Station.Name.Replace("-", "").Split(' ');
                return !string.IsNullOrEmpty(Station.Affiliate) && names.Length == 2 && names[0] == Station.Callsign && names[1].Equals($"({names[0]})");
            }
        }

        public LineupStation Station { get; private set; }
        public string StationId => Station.StationId;
        public string CallSign => Station.Callsign;
        public string LanguageCode => Station.BroadcastLanguage[0]?.ToLower().Substring(0, 2) ?? "zz";
        public string Name => (IsAtsc ? $"{CallSign} ({Station.Affiliate})" : null) ?? Station.Name;
        public bool IsNew { get; internal set; }

        // static station options
        public readonly SdChannelDownload StationOptions;
        public bool HDOverride => StationOptions.HdOverride;
        public bool SDOverride => StationOptions.SdOverride;
        public string CustomCallsign => StationOptions.CustomCallSign;
        public string CustomServiceName => StationOptions.CustomServiceName;

        // changeable station options
        private bool _include;
        public bool Include
        {
            get => !StationOptions.StationId.StartsWith("-");
            set
            {
                if (value == _include) return;
                if (value && !_include) StationOptions.StationId = StationId;
                if (!value && _include) StationOptions.StationId = "-" + StationId;
                _include = value;
                OnIncludeChanged(EventArgs.Empty);
            }
        }

        // logo presentation
        private Bitmap _serviceLogo;
        public Bitmap ServiceLogo
        {
            get => _serviceLogo;
            set
            {
                _serviceLogo = value;
                OnLogoChanged(EventArgs.Empty);
            }
        }

        // events
        public event EventHandler IncludeChanged;
        protected virtual void OnIncludeChanged(EventArgs e)
        {
            var handler = IncludeChanged;
            handler?.Invoke(this, e);
        }

        public event EventHandler LogoChanged;
        protected virtual void OnLogoChanged(EventArgs e)
        {
            var handler = LogoChanged;
            handler?.Invoke(this, e);
        }
    }
}