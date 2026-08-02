using System.Collections.Generic;

namespace Tracking.SDK.Commands;

public static class CommandCatalog
{
    public static readonly IReadOnlyList<DeviceCommandDefinition> Commands =
    [
        new(DeviceCommandNames.Position,   "Request current position"),
        new(DeviceCommandNames.Status,     "Request device status"),
        new(DeviceCommandNames.History,    "Request history"),
        new(DeviceCommandNames.Reboot,     "Reboot device"),

        new(DeviceCommandNames.OilOn,      "Enable fuel relay"),
        new(DeviceCommandNames.OilOff,     "Disable fuel relay"),

        new(DeviceCommandNames.RelayOn,    "Relay ON"),
        new(DeviceCommandNames.RelayOff,   "Relay OFF"),

        new(DeviceCommandNames.Monitor,    "Switch to monitor mode"),
        new(DeviceCommandNames.Tracker,    "Switch to tracker mode"),

        new(DeviceCommandNames.Upload,     "Set upload interval"),
        new(DeviceCommandNames.Check,      "Read configuration"),

        new(DeviceCommandNames.Apn,        "Configure APN"),
        new(DeviceCommandNames.Server,     "Configure Server"),
        new(DeviceCommandNames.TimeZone,   "Configure TimeZone"),
        new(DeviceCommandNames.Password,   "Change password"),

        new(DeviceCommandNames.Factory,    "Factory reset"),
        new(DeviceCommandNames.RestartGps, "Restart GPS"),
        new(DeviceCommandNames.Reset,      "Soft reset"),

        new(DeviceCommandNames.Sos,        "SOS settings"),
        new(DeviceCommandNames.Shock,      "Shock alarm"),
        new(DeviceCommandNames.Move,       "Move alarm"),
        new(DeviceCommandNames.Speed,      "Speed alarm"),
        new(DeviceCommandNames.Sleep,      "Sleep mode"),
        new(DeviceCommandNames.Heartbeat,  "Heartbeat interval"),

        new(DeviceCommandNames.Imei,       "Read IMEI"),
        new(DeviceCommandNames.Version,    "Read firmware version"),
        new(DeviceCommandNames.Signal,     "Read GSM signal"),
    ];
}