using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.GCSViews
{
    public partial class GaugeCluster : UserControl
    {
        private TableLayoutPanel layout;
        private Dictionary<string, Label> valueLabels = new Dictionary<string, Label>();

        public GaugeCluster()
        {
            InitLayout();
        }

        private void InitLayout()
        {
            layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6,
                AutoSize = true,
            };

            layout.ColumnStyles.Clear();
            layout.RowStyles.Clear();
            for (int i = 0; i < layout.ColumnCount; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / layout.ColumnCount));
            for (int i = 0; i < layout.RowCount; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / layout.RowCount));

            string[] labels =
            {
                "RPM", "Throttle %", "FPS (V)", "FPS (A)",
                "CubeT (C)", "Cube (V)", "VPS (V)", "VPS (A)",
                "OAT (C)", "VSI (ft/min)", "Dist. Home (nm)", "Time In Air",
                $"Airspeed ({CurrentState.SpeedUnit})", $"AGL ({CurrentState.AltUnit})", $"Laser Alt ({CurrentState.AltUnit})", "Link (%)",
                "Arm/Disarm", "Dist. Travel (nm)", "Sats / HDOP", $"Wind ({CurrentState.SpeedUnit})",
            };

            foreach (var label in labels)
            {
                var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Green };
                var nameLabel = new Label
                {
                    Text = label,
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold)
                };
                var valueLabel = new Label
                {
                    Text = "0",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold)
                };
                panel.Controls.Add(valueLabel);
                panel.Controls.Add(nameLabel);
                valueLabels[label] = valueLabel;
                layout.Controls.Add(panel);
            }

            Controls.Add(layout);
        }

        private CurrentState data;
        public CurrentState Data
        {
            get => data;
            set
            {
                data = value;
                if (data != null)
                {
                    UpdateData(data);
                }
            }
        }

        public void UpdateData(CurrentState state)
        {
            Func<double, double> toNauticalMiles = (double distance) => distance / (CurrentState.multiplierdist * 1852.0);

            UpdateLabel("RPM", state.rpm1, GetRPMColor(state.rpm1));
            UpdateLabel("Throttle %", state.ch3percent, state.ch3percent <= 95 ? Color.Green : Color.Orange);
            UpdateLabel("FPS (V)", state.battery_voltage, GetFPSVoltageColor(state.battery_voltage));
            UpdateLabel("FPS (A)", state.current, GetFPSCurrentColor(state.current));
            UpdateLabel("CubeT (C)", state.imu1_temp, GetAutopilotTempColor(state.imu1_temp));
            UpdateLabel("Cube (V)", state.boardvoltage, GetAutopilotVoltageColor(state.boardvoltage));
            UpdateLabel("VPS (V)", state.battery_voltage2, GetVPSVoltageColor(state.battery_voltage2));
            UpdateLabel("VPS (A)", state.current2, GetVPSCurrentColor(state.current2));
            UpdateLabel("OAT (C)", state.airspeed1_temp, GetOATColor(state.airspeed1_temp));
            UpdateLabel("VSI (ft/min)", state.verticalspeed_fpm, GetVSIColor(state.verticalspeed_fpm));
            UpdateLabel("Dist. Home (nm)", toNauticalMiles(state.DistToHome), Color.Green);
            UpdateLabel("Time In Air", TimeSpan.FromSeconds(state.timeInAirMinSec).ToString(@"mm\:ss"), Color.Green);
            UpdateLabel($"Airspeed ({CurrentState.SpeedUnit})", state.airspeed, GetAirspeedColor(state.airspeed));
            UpdateLabel($"AGL ({CurrentState.AltUnit})", state.alt, GetAGLColor(state.alt));
            UpdateLabel($"Laser Alt ({CurrentState.AltUnit})", state.sonarrange, Color.Green);
            UpdateLabel("Link (%)", state.linkqualitygcs, GetLinkQualityColor(state.linkqualitygcs));
            UpdateLabel("Arm/Disarm", state.armed ? "ARMED" : "DISARMED", state.armed ? Color.Green : Color.Red);
            UpdateLabel("Dist. Travel (nm)", toNauticalMiles(state.distTraveled), Color.Green);
            UpdateLabel("Sats / HDOP", $"{state.satcount} / {state.gpshdop}", GetGPSColor(state.satcount, state.gpshdop));
            UpdateLabel($"Wind ({CurrentState.SpeedUnit})", state.wind_vel, GetWindColor(state.wind_vel));
        }

        private void UpdateLabel(string key, double value, Color? backColor = null) => UpdateLabel(key, value.ToString("0.0"), backColor);

        private void UpdateLabel(string key, string text, Color? backColor = null)
        {
            if (valueLabels.TryGetValue(key, out var label))
            {
                label.Text = text;
                if (label.Parent is Panel panel && backColor.HasValue)
                    panel.BackColor = backColor.Value;
            }
        }

        private Color GetRPMColor(double rpm)
        {
            if (rpm < 2000) return Color.Orange;
            if (rpm <= 7500) return Color.Green;
            if (rpm <= 8500) return Color.Orange;
            return Color.Red;
        }

        private Color GetFPSVoltageColor(double v)
        {
            if (v < 42.0) return Color.Red;
            if (v <= 43.5) return Color.Orange;
            if (v <= 50.4) return Color.Green;
            if (v <= 50.5) return Color.Orange;
            return Color.Red;
        }

        private Color GetFPSCurrentColor(double a)
        {
            if (a < 85) return Color.Green;
            if (a <= 95) return Color.Orange;
            return Color.Red;
        }

        private Color GetAutopilotTempColor(double temp)
        {
            if (temp < 30) return Color.Red;
            if (temp <= 40) return Color.Orange;
            if (temp <= 50) return Color.Green;
            if (temp <= 65) return Color.Orange;
            return Color.Red;
        }

        private Color GetAutopilotVoltageColor(double v)
        {
            if (v < 4.4) return Color.Red;
            if (v <= 4.9) return Color.Orange;
            if (v <= 5.4) return Color.Green;
            if (v <= 5.5) return Color.Orange;
            return Color.Red;
        }

        private Color GetVPSVoltageColor(double v)
        {
            if (v < 42.2) return Color.Red;
            if (v <= 43.5) return Color.Orange;
            if (v <= 50.4) return Color.Green;
            if (v <= 50.5) return Color.Orange;
            return Color.Red;
        }

        private Color GetVPSCurrentColor(double a)
        {
            if (a <= 110) return Color.Green;
            if (a <= 120) return Color.Orange;
            return Color.Red;
        }

        private Color GetOATColor(double c)
        {
            if (c < -10) return Color.Red;
            if (c <= 0) return Color.Orange;
            if (c <= 35) return Color.Green;
            if (c <= 55) return Color.Orange;
            return Color.Red;
        }

        private Color GetVSIColor(double vsi)
        {
            if (vsi < -1200) return Color.Red;
            if (vsi < -1100) return Color.Orange;
            if (vsi <= 1100) return Color.Green;
            if (vsi <= 1500) return Color.Orange;
            return Color.Red;
        }

        private Color GetAirspeedColor(double spd)
        {
            if (spd < 35) return Color.Red;
            if (spd <= 40) return Color.Orange;
            if (spd <= 60) return Color.Green;
            if (spd <= 68) return Color.Orange;
            return Color.Red;
        }

        private Color GetAGLColor(double alt)
        {
            if (alt < 200) return Color.Red;
            if (alt <= 300) return Color.Orange;
            return Color.Green;
        }

        private Color GetLinkQualityColor(double quality)
        {
            if (quality < 80) return Color.Red;
            if (quality < 90) return Color.Orange;
            return Color.Green;
        }

        private Color GetGPSColor(double sats, double hdop)
        {
            if (sats < 6) return Color.Red;
            if (sats < 10) return Color.Orange;

            if (hdop > 2.0) return Color.Red;
            if (hdop > 1.2) return Color.Orange;

            return Color.Green;
        }

        private Color GetWindColor(double windSpeed)
        {
            if (windSpeed < 25) return Color.Green;
            if (windSpeed <= 35) return Color.Orange;
            return Color.Red;
        }
    }
}