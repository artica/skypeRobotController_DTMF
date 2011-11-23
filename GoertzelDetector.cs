
using System;
using System.Windows.Forms;


public class GoertzelDetector
{
    public const int GOERTZEL_SAMPLES = 190;
    private const int MAX_BINS = 8;
    public int SamplingRate = 16000;
    private int m_sampleCount;
    private double[] m_q1 = new double[MAX_BINS];
    private double[] m_q2 = new double[MAX_BINS];
    private double[] m_r = new double[MAX_BINS];
    private double[] m_coefs = new double[MAX_BINS];
    private double[] m_freqs = new double[] { 697, 770, 852, 941, 1209, 1336, 1477, 1633 };

    public GoertzelDetector()
    {
        // Always use Try/Catch logic it's just good coding practice.
        try
        {
            calc_coeffs();
        }
        catch (Exception e)
        {
            // Write that public calculations of Goertzel coefficients failed.
            MessageBox.Show(DateTime.Now.ToLocalTime() +
                            ": " +
                            "Public calculations of Goertzel coefficients failed: " +
                            " - Exception Source: " +
                            e.Source +
                            " - Exception Message: " +
                            e.Message);
        }
    }

    private void calc_coeffs()
    {
        // Always use Try/Catch logic it's just good coding practice.
        try
        {
            for (int n = 0; n < MAX_BINS; n++)
                m_coefs[n] = 2.0 * Math.Cos(2.0 * 3.141592654 * m_freqs[n] / SamplingRate);
        }
        catch (Exception e)
        {
            // Write that private calculations of Goertzel coefficients failed.
            MessageBox.Show(DateTime.Now.ToLocalTime() +
                            ": " +
                            "Private calculations of Goertzel coefficients failed: " +
                            " - Exception Source: " +
                            e.Source +
                            " - Exception Message: " +
                            e.Message);
        }
    }

    private char post_testing()
    {
        // Always use Try/Catch logic it's just good coding practice.
        try
        {
            bool see_digit = false;
            int row, col;
            int peak_count, max_index;
            double maxval, t;
            int i;

            char[][] row_col_ascii_codes = new char[][]{
                    new char[]{'1', '2', '3', char.MinValue},
                    new char[]{'4', '5', '6', char.MinValue},
                    new char[]{'7', '8', '9', char.MinValue},
                    new char[]{'*', '0', '#', char.MinValue}};

            row = 0;
            maxval = 0.0;

            for (i = 0; i < 4; i++)
                if (m_r[i] > maxval)
                {
                    maxval = m_r[i];
                    row = i;
                }

            col = 4;
            maxval = 0.0;

            for (i = 4; i < 8; i++)
                if (m_r[i] > maxval)
                {
                    maxval = m_r[i];
                    col = i;
                }

            if (m_r[row] < 4.0e5)
            {
            }
            else if (m_r[col] < 4.0e5)
            {
            }
            else
            {
                see_digit = true;

                if (m_r[col] > m_r[row])
                {
                    max_index = col;
                    if (m_r[row] < (m_r[col] * 0.398))
                        see_digit = false;
                }
                else
                {
                    max_index = row;
                    if (m_r[col] < (m_r[row] * 0.158))
                        see_digit = false;
                }

                if (m_r[max_index] > 1.0e9)
                    t = m_r[max_index] * 0.158;
                else
                    t = m_r[max_index] * 0.010;

                peak_count = 0;

                for (i = 0; i < 8; i++)
                    if (m_r[i] > t)
                        peak_count++;

                if (peak_count > 2)
                    see_digit = false;

                if (see_digit)
                    return row_col_ascii_codes[row][col - 4];
                else
                    return char.MinValue;
            }

            return char.MinValue;
        }
        catch (Exception e)
        {
            // Write that Private Goertzel post_testing failed.
            MessageBox.Show(DateTime.Now.ToLocalTime() +
                            ": " +
                            "Private Goertzel post_testing failed: " +
                            " - Exception Source: " +
                            e.Source +
                            " - Exception Message: " +
                            e.Message);

            return char.MinValue;
        }
    }

    public char ProcessSample(int sample)
    {
        // Always use Try/Catch logic it's just good coding practice.
        try
        {
            if (m_sampleCount < GOERTZEL_SAMPLES)
            {
                m_sampleCount++;

                for (int i = 0; i < MAX_BINS; i++)
                {
                    double q0 = m_coefs[i] * m_q1[i] - m_q2[i] + sample;
                    m_q2[i] = m_q1[i];
                    m_q1[i] = q0;
                }

                return char.MinValue;
            }
            else
            {
                for (int i = 0; i < MAX_BINS; i++)
                {
                    m_r[i] = (m_q1[i] * m_q1[i]) + (m_q2[i] * m_q2[i]) - (m_coefs[i] * m_q1[i] * m_q2[i]);
                    m_q1[i] = 0.0;
                    m_q2[i] = 0.0;
                }

                m_sampleCount = 0;

                return post_testing();
            }
        }
        catch (Exception e)
        {
            // Write that Public Goertzel process sample failed.
            MessageBox.Show(DateTime.Now.ToLocalTime() +
                            ": " +
                            "Public Goertzel ProcessSample failed: " +
                            " - Exception Source: " +
                            e.Source +
                            " - Exception Message: " +
                            e.Message);

            return char.MinValue;
        }
    }
}
