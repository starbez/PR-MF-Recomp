using System;

namespace PR.Core;

public sealed class ActorState
{
    public int PrevX;
    public int PrevY;
    public int CurrX;
    public int CurrY;
    public bool Initialized;
    public bool Snapped;

    public void Update(int newX, int newY)
    {
        if (!Initialized || Snapped)
        {
            PrevX = newX;
            PrevY = newY;
            CurrX = newX;
            CurrY = newY;
            Initialized = true;
            Snapped = false;
            return;
        }

        // Teleport threshold: if actor or camera jumped more than 64 pixels in one tick, snap
        if (Math.Abs(newX - CurrX) > 64 || Math.Abs(newY - CurrY) > 64)
        {
            PrevX = newX;
            PrevY = newY;
        }
        else
        {
            PrevX = CurrX;
            PrevY = CurrY;
        }

        CurrX = newX;
        CurrY = newY;
    }

    public void Snap(int x, int y)
    {
        PrevX = x;
        PrevY = y;
        CurrX = x;
        CurrY = y;
        Initialized = true;
        Snapped = true;
    }

    public (int x, int y) GetInterpolated(float alpha)
    {
        if (!Initialized) return (CurrX, CurrY);
        float a = Math.Clamp(alpha, 0.0f, 1.0f);
        int ix = (int)Math.Round(PrevX + (CurrX - PrevX) * a);
        int iy = (int)Math.Round(PrevY + (CurrY - PrevY) * a);
        return (ix, iy);
    }
}
