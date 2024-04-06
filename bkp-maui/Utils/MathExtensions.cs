namespace d9.bkp.maui;

public static class MathExtensions
{
    public static int Digits(this long l)
    {
        if (l < 0)
            l = -l;
        int ct = 0;
        while (l > 9)
        {
            l /= 10;
            ct++;
        }
        return ct;
    }
    public static int Divisor(this int digits)
    {
        int result = 1;
        for (int i = 0; i < digits / 3; i++)
            result *= 1000;
        return result;
    }
}
