// triage_core.v - Modul Logika Digital Triage Stroke
module triage_core (
    input wire CLK,
    input wire RST,
    input wire [5:0] S, // Sensor Input S5..S0
    output reg [5:0] A  // Actuator Output A5..A0
);

// State Register (2-bit FSM: Q1 Q0)
reg [1:0] PS, NS; // Present State, Next State

// Definisi State
parameter IDLE = 2'b00;
parameter OBSERVATION = 2'b01;
parameter PRE_CRITICAL = 2'b10;
parameter CRITICAL = 2'b11;

// Sinyal Transisi Internal
wire N_sig, L1_sig, H_sig;

// --- LOGIKA KOMBINASIONAL (INPUT S -> N, L1, H) ---
// N (Normal): Semua S = 0
assign N_sig = ~(|S);

// H (Risiko Tinggi): S0S1 + S2S3 + S0S2
assign H_sig = (S[0] & S[1]) | (S[2] & S[3]) | (S[0] & S[2]);

// L1 (Risiko Rendah): S5 + S4, dan BUKAN H
assign L1_sig = (S[5] | S[4]) & ~H_sig;

// --- LOGIKA STATE TRANSISI (NEXT STATE LOGIC) ---
always @(*) begin
    NS = PS; 
    case (PS)
        IDLE: begin
            if (H_sig) NS = PRE_CRITICAL;
            else if (L1_sig) NS = OBSERVATION;
            else NS = IDLE;
        end
        OBSERVATION: begin
            if (H_sig) NS = PRE_CRITICAL;
            else if (N_sig) NS = IDLE;
            else NS = OBSERVATION;
        end
        PRE_CRITICAL: begin
            if (H_sig | L1_sig) NS = CRITICAL; 
            else if (N_sig) NS = IDLE;
            else NS = PRE_CRITICAL;
        end
        CRITICAL: begin
            NS = CRITICAL; 
        end
        default: NS = IDLE;
    endcase
end

// --- LOGIKA SEQUENTIAL (UPDATE STATE DENGAN CLOCK) ---
always @(posedge CLK or posedge RST) begin
    if (RST)
        PS <= IDLE;
    else
        PS <= NS;
end

// --- LOGIKA OUTPUT AKTUATOR (A5..A0) ---
always @(*) begin
    case (PS)
        IDLE: A = 6'b000000;
        OBSERVATION: A = 6'b001100; 
        PRE_CRITICAL: A = 6'b011010; 
        CRITICAL: A = 6'b111111; 
        default: A = 6'b000000;
    endcase
end

endmodule