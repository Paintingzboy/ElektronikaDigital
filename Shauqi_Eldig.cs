using System;
using System.Linq;

public class QuantumTriageSimulator
{
    // Array 64 elemen merepresentasikan amplitudo kuantum
    private double[] Amplitudes = new double[64];

    // 1. Definisikan Logika Kritis (Oracle Klasik) - Menggunakan Logika H Anda: S0S1 + S2S3 + S0S2
    private bool IsCritical(int sensor_index)
    {
        // sensor_index adalah representasi biner 6-bit (0 hingga 63) dari S5 S4 S3 S2 S1 S0.

        // Ekstraksi bit sensor dari indeks:
        // S0 adalah bit ke-0 (LSB)
        int S0 = (sensor_index >> 0) & 1; 
        int S1 = (sensor_index >> 1) & 1;
        // int S2 = (sensor_index >> 2) & 1; // S2, S3 digunakan di logika H
        // int S3 = (sensor_index >> 3) & 1;
        // int S4 = (sensor_index >> 4) & 1; // S4, S5 tidak digunakan di logika H
        // int S5 = (sensor_index >> 5) & 1;
        
        // Karena S2 dan S3 sudah digunakan untuk ekstraksi di bawah:
        int S2 = (sensor_index >> 2) & 1;
        int S3 = (sensor_index >> 3) & 1;
        
        // Logika High Risk (H): H = S0S1 + S2S3 + S0S2 (menggunakan gerbang OR dan AND)
        // Di C#: (S0 & S1) | (S2 & S3) | (S0 & S2)
        
        // Catatan: Operasi bitwise '&' dan '|' digunakan untuk AND dan OR Boolean
        bool H_is_satisfied = ((S0 & S1) == 1) || ((S2 & S3) == 1) || ((S0 & S2) == 1);

        return H_is_satisfied; 
    }

    // 3b. Simulasi Difusi (Amplifikasi)
    // Operasi ini: Amplitudo[x] = 2 * Mean - Amplitudo[x]
    private void DiffusionOperator()
    {
        // Hitung nilai rata-rata (Mean) dari semua amplitudo
        double mean_amplitude = Amplitudes.Sum() / 64.0;

        // Terapkan operasi difusi
        for (int x = 0; x < 64; x++)
        {
            Amplitudes[x] = (2.0 * mean_amplitude) - Amplitudes[x];
        }
    }

    public void RunGroverSimulation(int iterations)
    {
        // 2. Inisialisasi Superposisi
        double initial_amp = 1.0 / Math.Sqrt(64);
        for (int x = 0; x < 64; x++)
        {
            Amplitudes[x] = initial_amp; // Atur semua amplitudo sama
        }
        Console.WriteLine($"Inisialisasi {64} keadaan dalam superposisi (Amplitudo: {initial_amp:F4})");
        Console.WriteLine($"Keadaan Kritis (Target) ditemukan: {Amplitudes.Count(x => IsCritical(x))} dari 64.");
        Console.WriteLine($"Mulai iterasi Grover ({iterations} putaran).");

        for (int i = 0; i < iterations; i++)
        {
            // 3a. Simulasi Oracle (Membalik Fase)
            for (int x = 0; x < 64; x++)
            {
                if (IsCritical(x))
                {
                    Amplitudes[x] *= -1; // Fase-flip (Oracle Uf)
                }
            }
            
            // 3b. Simulasi Difusi (Amplifikasi)
            DiffusionOperator();
            
            // Opsional: Cetak Probabilitas Tertinggi di setiap putaran
            // Console.WriteLine($"Iterasi {i + 1} selesai.");
        }

        // 4. Pengukuran (Cari Amplitudo Tertinggi)
        Console.WriteLine("\n--- Hasil Pengukuran Setelah Iterasi ---");
        
        // Hitung probabilitas (Amplitudo kuadrat)
        double[] Probabilities = Amplitudes.Select(amp => amp * amp).ToArray();

        // Cari indeks dengan probabilitas tertinggi
        double max_prob = Probabilities.Max();
        int most_likely_index = Array.IndexOf(Probabilities, max_prob);
        
        // Konversi indeks kembali ke representasi biner 6-bit
        string binary_state = Convert.ToString(most_likely_index, 2).PadLeft(6, '0');

        Console.WriteLine($"Probabilitas Tertinggi: {max_prob * 100:F2}%");
        Console.WriteLine($"Kombinasi Sensor yang Paling Mungkin Kritis (S5..S0): {binary_state}");
        Console.WriteLine($"(Indeks Desimal: {most_likely_index})");
        
        // Konfirmasi apakah hasil yang diamplifikasi adalah keadaan kritis
        if (IsCritical(most_likely_index))
        {
             Console.WriteLine("VALIDASI: Keadaan sensor yang paling mungkin ini memang memenuhi logika H.");
        }
        else
        {
             Console.WriteLine("VALIDASI: Peringatan, Keadaan yang diamplifikasi bukan keadaan Kritis H. (Perlu penyesuaian iterasi)");
        }
    }
}

// Contoh Penggunaan:
// public static void Main(string[] args)
// {
//     QuantumTriageSimulator simulator = new QuantumTriageSimulator();
//     // Jumlah iterasi yang optimal untuk N=64 adalah sekitar pi/4 * sqrt(N) = 6.28.
//     // Kita coba 6 iterasi.
//     simulator.RunGroverSimulation(6);
// }