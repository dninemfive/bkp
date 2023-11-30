﻿namespace d9.bkp.maui;
public static class Utils
{
    // cached to avoid issues when running near midnight
    private static DateTime? _today = null;
    public static string DateToday
    {
        get
        {
            _today ??= DateTime.Today;
            return _today.Value.ToString("yyyy.M.d");
        }
    }
}
