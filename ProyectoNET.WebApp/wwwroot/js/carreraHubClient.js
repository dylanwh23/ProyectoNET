/*
export class CarreraHubClient {
    constructor(carreraId, hubUrl = "https://localhost:7252/carreraHub") {
        if (!carreraId) throw new Error("Debe especificarse el ID de la carrera");
        this.carreraId = carreraId;
        this.HUB_URL = hubUrl;
        this.connection = null;

        // Callbacks
        this.onProgreso = null;
        this.onLog = null;
        this.onReconnect = null;
        this.onClose = null;
    }

    async start() {
        if (!window.signalR) throw new Error("⚠️ Falta la librería SignalR (signalr.min.js)");
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.HUB_URL)
            .withAutomaticReconnect()
            .build();

        // Evento de progreso
        this.connection.on("RecibirProgreso", data => {
            if (data.carreraId === this.carreraId) {
                if (this.onProgreso) this.onProgreso(data);
                if (this.onLog) this.onLog(`📥 Carrera ${data.carreraId} | Corredor ${data.corredorId} | ${data.checkpoint} | Velocidad ${data.velocidad.toFixed(2)} km/h | Tramos ${data.tramosCompletados}`);
            }
        });

        // Reconexión automática
        this.connection.onreconnecting(error => {
            if (this.onLog) this.onLog(`🔄 Reconectando: ${error?.message}`);
        });

        this.connection.onreconnected(async id => {
            if (this.onLog) this.onLog(`✅ Reconectado (${id})`);
            await this.connection.invoke("UnirseCarrera", this.carreraId);
            if (this.onLog) this.onLog(`✅ Reunido al grupo carrera-${this.carreraId} tras reconectar`);
            if (this.onReconnect) this.onReconnect(id);
        });

        this.connection.onclose(err => {
            if (this.onLog) this.onLog(`❌ Desconectado: ${err?.message}`);
            if (this.onClose) this.onClose(err);
        });

        try {
            if (this.onLog) this.onLog("▶️ Iniciando conexión...");
            await this.connection.start();
            if (this.onLog) this.onLog(`✅ Conectado con ID: ${this.connection.connectionId}`);
            await this.connection.invoke("UnirseCarrera", this.carreraId);
            if (this.onLog) this.onLog(`✅ Unido al grupo carrera-${this.carreraId}`);
        } catch (err) {
            if (this.onLog) this.onLog(`⚠️ Error al conectar: ${err.message}`);
            setTimeout(() => this.start(), 5000);
        }
    }

    async stop() {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.stop();
            if (this.onLog) this.onLog("🛑 Conexión detenida");
        }
    }
}
*/