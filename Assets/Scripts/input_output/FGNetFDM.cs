using System;
using System.Runtime.InteropServices;

public static class FGNetConstants
{
    public const int FG_MAX_ENGINES = 4;
    public const int FG_MAX_WHEELS = 3;
    public const int FG_MAX_TANKS = 4;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FGNetFDM
{
    public uint version;
    public uint padding;

    // Positions
    public double longitude;
    public double latitude;
    public double altitude;
    public float agl;
    public float phi;
    public float theta;
    public float psi;
    public float alpha;
    public float beta;

    // Velocities
    public float phidot;
    public float thetadot;
    public float psidot;
    public float vcas;
    public float climb_rate;
    public float v_north;
    public float v_east;
    public float v_down;
    public float v_body_u;
    public float v_body_v;
    public float v_body_w;

    // Accelerations
    public float A_X_pilot;
    public float A_Y_pilot;
    public float A_Z_pilot;

    // Stall
    public float stall_warning;
    public float slip_deg;

    // Engine status
    public uint num_engines;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public uint[] eng_state;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] rpm;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] fuel_flow;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] fuel_px;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] egt;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] cht;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] mp_osi;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] tit;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] oil_temp;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_ENGINES)]
    public float[] oil_px;

    // Consumables
    public uint num_tanks;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public float[] fuel_quantity;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public uint[] tank_selected;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public double[] capacity_m3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public double[] unusable_m3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public double[] density_kgpm3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_TANKS)]
    public double[] level_m3;

    // Gear status
    public uint num_wheels;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_WHEELS)]
    public uint[] wow;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_WHEELS)]
    public float[] gear_pos;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_WHEELS)]
    public float[] gear_steer;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = FGNetConstants.FG_MAX_WHEELS)]
    public float[] gear_compression;

    // Environment
    public uint cur_time;
    public int warp;
    public float visibility;

    // Control surface positions (normalized values)
    public float elevator;
    public float elevator_trim_tab;
    public float left_flap;
    public float right_flap;
    public float left_aileron;
    public float right_aileron;
    public float rudder;
    public float nose_wheel;
    public float speedbrake;
    public float spoilers;

    public void SwapEndian()
    {
        // Swap the endianness of all the elements
        version = SwapBytes(version);
        padding = SwapBytes(padding);

        longitude = SwapBytes(longitude);
        latitude = SwapBytes(latitude);
        altitude = SwapBytes(altitude);
        agl = SwapBytes(agl);
        phi = SwapBytes(phi);
        theta = SwapBytes(theta);
        psi = SwapBytes(psi);
        alpha = SwapBytes(alpha);
        beta = SwapBytes(beta);

        phidot = SwapBytes(phidot);
        thetadot = SwapBytes(thetadot);
        psidot = SwapBytes(psidot);
        vcas = SwapBytes(vcas);
        climb_rate = SwapBytes(climb_rate);
        v_north = SwapBytes(v_north);
        v_east = SwapBytes(v_east);
        v_down = SwapBytes(v_down);
        v_body_u = SwapBytes(v_body_u);
        v_body_v = SwapBytes(v_body_v);
        v_body_w = SwapBytes(v_body_w);

        A_X_pilot = SwapBytes(A_X_pilot);
        A_Y_pilot = SwapBytes(A_Y_pilot);
        A_Z_pilot = SwapBytes(A_Z_pilot);

        stall_warning = SwapBytes(stall_warning);
        slip_deg = SwapBytes(slip_deg);

        num_engines = SwapBytes(num_engines);
        SwapBytesArray(ref eng_state);
        SwapBytesArray(ref rpm);
        SwapBytesArray(ref fuel_flow);
        SwapBytesArray(ref fuel_px);
        SwapBytesArray(ref egt);
        SwapBytesArray(ref cht);
        SwapBytesArray(ref mp_osi);
        SwapBytesArray(ref tit);
        SwapBytesArray(ref oil_temp);
        SwapBytesArray(ref oil_px);

        num_tanks = SwapBytes(num_tanks);
        SwapBytesArray(ref fuel_quantity);
        SwapBytesArray(ref tank_selected);
        SwapBytesArray(ref capacity_m3);
        SwapBytesArray(ref unusable_m3);
        SwapBytesArray(ref density_kgpm3);
        SwapBytesArray(ref level_m3);

        num_wheels = SwapBytes(num_wheels);
        SwapBytesArray(ref wow);
        SwapBytesArray(ref gear_pos);
        SwapBytesArray(ref gear_steer);
        SwapBytesArray(ref gear_compression);

        cur_time = SwapBytes(cur_time);
        warp = SwapBytes(warp);
        visibility = SwapBytes(visibility);

        elevator = SwapBytes(elevator);
        elevator_trim_tab = SwapBytes(elevator_trim_tab);
        left_flap = SwapBytes(left_flap);
        right_flap = SwapBytes(right_flap);
        left_aileron = SwapBytes(left_aileron);
        right_aileron = SwapBytes(right_aileron);
        rudder = SwapBytes(rudder);
        nose_wheel = SwapBytes(nose_wheel);
        speedbrake = SwapBytes(speedbrake);
        spoilers = SwapBytes(spoilers);
    }

    private uint SwapBytes(uint x)
    {
        return (x << 24) |
               ((x << 8) & 0x00FF0000) |
               ((x >> 8) & 0x0000FF00) |
               (x >> 24);
    }

    private int SwapBytes(int x)
    {
        return (int)SwapBytes((uint)x);
    }

    private double SwapBytes(double x)
    {
        byte[] bytes = BitConverter.GetBytes(x);
        Array.Reverse(bytes);
        return BitConverter.ToDouble(bytes, 0);
    }

    private float SwapBytes(float x)
    {
        byte[] bytes = BitConverter.GetBytes(x);
        Array.Reverse(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }

    private void SwapBytesArray<T>(ref T[] array) where T : struct
    {
        if (array == null)
        {
            return;
        }

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = SwapBytes(array[i]);
        }
    }

    private T SwapBytes<T>(T x) where T : struct
    {
        Type type = typeof(T);
        if (type == typeof(uint))
            return (T)(object)SwapBytes((uint)(object)x);
        if (type == typeof(int))
            return (T)(object)SwapBytes((int)(object)x);
        if (type == typeof(double))
            return (T)(object)SwapBytes((double)(object)x);
        if (type == typeof(float))
            return (T)(object)SwapBytes((float)(object)x);
        
        throw new ArgumentException("Unsupported type for endian swap");
    }
}
