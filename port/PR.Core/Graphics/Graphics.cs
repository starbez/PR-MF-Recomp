using System;

namespace PR.Core.Graphics;

public sealed class Graphics
{
    public const int HCENTER = 1;
    public const int VCENTER = 2;
    public const int LEFT = 4;
    public const int RIGHT = 8;
    public const int TOP = 16;
    public const int BOTTOM = 32;
    public const int BASELINE = 64;

    public const int TRANS_NONE = 0;
    public const int TRANS_MIRROR_ROT180 = 1;
    public const int TRANS_MIRROR = 2;
    public const int TRANS_ROT180 = 3;
    public const int TRANS_MIRROR_ROT270 = 4;
    public const int TRANS_ROT90 = 5;
    public const int TRANS_ROT270 = 6;
    public const int TRANS_MIRROR_ROT90 = 7;

    private readonly Image _target;
    private int _transX;
    private int _transY;
    private uint _color = 0xFF000000;

    // Device coordinates for clipping
    private int _clipX;
    private int _clipY;
    private int _clipW;
    private int _clipH;

    public Graphics(Image target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _clipX = 0;
        _clipY = 0;
        _clipW = _target.Width;
        _clipH = _target.Height;
    }

    public void Translate(int x, int y)
    {
        _transX += x;
        _transY += y;
    }

    public int GetTranslateX() => _transX;
    public int GetTranslateY() => _transY;

    public void SetColor(int rgb)
    {
        _color = 0xFF000000 | ((uint)rgb & 0xFFFFFF);
    }

    public void SetColor(int r, int g, int b)
    {
        _color = 0xFF000000 | (((uint)r & 0xFF) << 16) | (((uint)g & 0xFF) << 8) | ((uint)b & 0xFF);
    }

    public int GetColor() => (int)(_color & 0xFFFFFF);

    public void SetClip(int x, int y, int width, int height)
    {
        int devX = _transX + x;
        int devY = _transY + y;
        int devX2 = devX + width;
        int devY2 = devY + height;

        int targetX1 = Math.Max(0, devX);
        int targetY1 = Math.Max(0, devY);
        int targetX2 = Math.Min(_target.Width, devX2);
        int targetY2 = Math.Min(_target.Height, devY2);

        _clipX = targetX1;
        _clipY = targetY1;
        _clipW = Math.Max(0, targetX2 - targetX1);
        _clipH = Math.Max(0, targetY2 - targetY1);
    }

    public void ClipRect(int x, int y, int width, int height)
    {
        int devX1 = _transX + x;
        int devY1 = _transY + y;
        int devX2 = devX1 + width;
        int devY2 = devY1 + height;

        int curX1 = _clipX;
        int curY1 = _clipY;
        int curX2 = _clipX + _clipW;
        int curY2 = _clipY + _clipH;

        int newX1 = Math.Max(curX1, devX1);
        int newY1 = Math.Max(curY1, devY1);
        int newX2 = Math.Min(curX2, devX2);
        int newY2 = Math.Min(curY2, devY2);

        _clipX = newX1;
        _clipY = newY1;
        _clipW = Math.Max(0, newX2 - newX1);
        _clipH = Math.Max(0, newY2 - newY1);
    }

    public int GetClipX() => _clipX - _transX;
    public int GetClipY() => _clipY - _transY;
    public int GetClipWidth() => _clipW;
    public int GetClipHeight() => _clipH;

    public void FillRect(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        int devX = _transX + x;
        int devY = _transY + y;

        int x1 = Math.Max(_clipX, devX);
        int y1 = Math.Max(_clipY, devY);
        int x2 = Math.Min(_clipX + _clipW, devX + width);
        int y2 = Math.Min(_clipY + _clipH, devY + height);

        if (x1 >= x2 || y1 >= y2) return;

        int tWidth = _target.Width;
        uint[] pixels = _target.Pixels;
        uint color = _color;

        for (int py = y1; py < y2; py++)
        {
            int row = py * tWidth;
            for (int px = x1; px < x2; px++)
            {
                pixels[row + px] = color;
            }
        }
    }

    public void ClearRect(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        int devX = _transX + x;
        int devY = _transY + y;

        int x1 = Math.Max(_clipX, devX);
        int y1 = Math.Max(_clipY, devY);
        int x2 = Math.Min(_clipX + _clipW, devX + width);
        int y2 = Math.Min(_clipY + _clipH, devY + height);

        if (x1 >= x2 || y1 >= y2) return;

        int tWidth = _target.Width;
        uint[] pixels = _target.Pixels;

        for (int py = y1; py < y2; py++)
        {
            int row = py * tWidth;
            Array.Clear(pixels, row + x1, x2 - x1);
        }
    }

    public void DrawRect(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        FillRect(x, y, width + 1, 1);
        FillRect(x, y + height, width + 1, 1);
        FillRect(x, y, 1, height + 1);
        FillRect(x + width, y, 1, height + 1);
    }

    public void DrawLine(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        int cx = x1;
        int cy = y1;

        while (true)
        {
            int devX = _transX + cx;
            int devY = _transY + cy;

            if (devX >= _clipX && devX < _clipX + _clipW &&
                devY >= _clipY && devY < _clipY + _clipH)
            {
                _target.Pixels[devY * _target.Width + devX] = _color;
            }

            if (cx == x2 && cy == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                cx += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                cy += sy;
            }
        }
    }

    public void DrawImage(Image img, int x, int y, int anchor)
    {
        if (img == null) return;
        DrawRegion(img, 0, 0, img.Width, img.Height, TRANS_NONE, x, y, anchor);
    }

    public void DrawRegion(Image src, int x_src, int y_src, int width, int height,
                           int transform, int x_dest, int y_dest, int anchor)
    {
        if (src == null || width <= 0 || height <= 0) return;

        int destW = width;
        int destH = height;
        if (transform == 5 || transform == 90 || transform == 6 || transform == 270 || transform == 4 || transform == 7) // 90 or 270 degree rotation
        {
            destW = height;
            destH = width;
        }

        int dx = x_dest;
        int dy = y_dest;

        if ((anchor & HCENTER) != 0) dx -= destW / 2;
        else if ((anchor & RIGHT) != 0) dx -= destW;

        if ((anchor & VCENTER) != 0) dy -= destH / 2;
        else if ((anchor & BOTTOM) != 0) dy -= destH;

        int devX = _transX + dx;
        int devY = _transY + dy;

        int x1 = Math.Max(_clipX, devX);
        int y1 = Math.Max(_clipY, devY);
        int x2 = Math.Min(_clipX + _clipW, devX + destW);
        int y2 = Math.Min(_clipY + _clipH, devY + destH);

        if (x1 >= x2 || y1 >= y2) return;

        int tWidth = _target.Width;
        uint[] dstPixels = _target.Pixels;
        int sWidth = src.Width;
        uint[] srcPixels = src.Pixels;

        for (int py = y1; py < y2; py++)
        {
            int rowDst = py * tWidth;
            int localY = py - devY;

            for (int px = x1; px < x2; px++)
            {
                int localX = px - devX;
                int sx, sy;

                switch (transform)
                {
                    case TRANS_MIRROR_ROT180: // 1
                        sx = x_src + localX;
                        sy = y_src + (height - 1 - localY);
                        break;
                    case TRANS_MIRROR: // 2
                        sx = x_src + (width - 1 - localX);
                        sy = y_src + localY;
                        break;
                    case TRANS_ROT180: // 3
                    case 180:
                        sx = x_src + (width - 1 - localX);
                        sy = y_src + (height - 1 - localY);
                        break;
                    case TRANS_ROT90: // 5
                    case 90:
                        sx = x_src + localY;
                        sy = y_src + (height - 1 - localX);
                        break;
                    case TRANS_ROT270: // 6
                    case 270:
                        sx = x_src + (width - 1 - localY);
                        sy = y_src + localX;
                        break;
                    case TRANS_MIRROR_ROT90: // 7
                        sx = x_src + (width - 1 - localY);
                        sy = y_src + (height - 1 - localX);
                        break;
                    case TRANS_MIRROR_ROT270: // 4
                        sx = x_src + localY;
                        sy = y_src + localX;
                        break;
                    case TRANS_NONE: // 0
                    default:
                        sx = x_src + localX;
                        sy = y_src + localY;
                        break;
                }

                if (sx < 0 || sx >= src.Width || sy < 0 || sy >= src.Height) continue;

                uint sp = srcPixels[sy * sWidth + sx];
                uint sa = sp >> 24;
                if (sa == 0) continue;

                if (sa == 255)
                {
                    dstPixels[rowDst + px] = sp;
                }
                else
                {
                    uint dp = dstPixels[rowDst + px];
                    uint da = dp >> 24;

                    uint sr = (sp >> 16) & 0xFF;
                    uint sg = (sp >> 8) & 0xFF;
                    uint sb = sp & 0xFF;

                    uint dr = (dp >> 16) & 0xFF;
                    uint dg = (dp >> 8) & 0xFF;
                    uint db = dp & 0xFF;

                    uint invA = 255 - sa;
                    uint outR = (sr * sa + dr * invA) / 255;
                    uint outG = (sg * sa + dg * invA) / 255;
                    uint outB = (sb * sa + db * invA) / 255;
                    uint outA = sa + (da * invA) / 255;

                    dstPixels[rowDst + px] = (outA << 24) | (outR << 16) | (outG << 8) | outB;
                }
            }
        }
    }
}
