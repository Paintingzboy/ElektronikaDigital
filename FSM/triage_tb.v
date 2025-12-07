// triage_tb.v - Testbench for triage_core
`timescale 1ns/1ns // Mendefinisikan skala waktu
module triage_tb;

// Sinyal Internal untuk dihubungkan ke modul utama
reg CLK;
reg RST;
reg [5:0] S_in; // Sensor Input
wire [5:0] A_out; // Actuator Output

// Instansiasi Modul Triage Core (UUT = Unit Under Test)
triage_core UUT (
    .CLK(CLK),
    .RST(RST),
    .S(S_in),
    .A(A_out)
);

// --- CLOCK GENERATION (Pembangkitan Clock) ---
parameter T = 100; // Periode Clock 100ns (Frekuensi 10MHz)
initial begin
    CLK = 0;
    forever #(T/2) CLK = ~CLK; // Clock berayun setiap 50ns
end

// --- DUMP FILE (Merekam Sinyal) ---
initial begin
    $dumpfile("triage_wave.vcd"); // Nama file yang akan dibuka GTKWave
    $dumpvars(0, triage_tb); // Merekam semua sinyal di testbench
end

// --- STIMULUS (Skenario Uji) ---
initial begin
    // 1. Reset Awal
    $display("Waktu: %0d | Sinyal: Reset Awal", $time);
    RST = 1; S_in = 6'b000000; #(T); // Tahan Reset selama 100ns
    RST = 0; #(T); // Lepaskan Reset
    
    // 2. Skenario IDLE (Normal)
    $display("Waktu: %0d | Sinyal: IDLE", $time);
    S_in = 6'b000000; // N=1. Tetap di IDLE (00). A = 000000
    #(5*T); 

    // 3. Skenario L1 (Risiko Rendah) - Jatuh (S5=1)
    $display("Waktu: %0d | Sinyal: L1 (Jatuh)", $time);
    S_in = 6'b100000; // L1=1. Transisi ke OBSERVATION (01). A = 001100
    #(2*T); 
    
    // 4. Skenario H (Risiko Tinggi) - Syok
    $display("Waktu: %0d | Sinyal: H (Syok)", $time);
    S_in = 6'b000110; // H=1. Transisi ke PRE_CRITICAL (10). A = 011010
    #(5*T);
    
    // 5. Skenario Lock State (Tidak Ada Perbaikan H berlanjut)
    $display("Waktu: %0d | Sinyal: Lock State (Critical)", $time);
    // H=1 berlanjut di PRE_CRITICAL. Transisi ke CRITICAL (11). A = 111111
    #(5*T); 

    // 6. Akhir Simulasi
    $display("Waktu: %0d | Sinyal: Simulasi Selesai", $time);
    $finish;
end

endmodule