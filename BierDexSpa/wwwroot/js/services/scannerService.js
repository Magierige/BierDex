// src/services/scannerService.js
export class ScannerService {
    constructor(elementId) {
        this.html5QrCode = new Html5Qrcode(elementId);
    }

    async start(onSuccess) {
        const config = {
            fps: 20,
            qrbox: { width: 250, height: 150 },
            aspectRatio: 1.0,
            formatsToSupport: [Html5QrcodeSupportedFormats.EAN_13]
        };

        try {
            await this.html5QrCode.start(
                { facingMode: "environment" },
                config,
                (decodedText) => {
                    this.stop();
                    onSuccess(decodedText);
                },
                () => { /* negeer fouten tijdens scannen */ }
            );
        } catch (err) {
            console.error("Camera error:", err);
            throw err;
        }
    }

    async stop() {
        if (this.html5QrCode && this.html5QrCode.isScanning) {
            await this.html5QrCode.stop();
            document.getElementById(this.html5QrCode.elementId)?.classList.add("hidden");
        }
    }
}