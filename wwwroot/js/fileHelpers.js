// Ensure functions are available globally for Blazor JS interop
window.saveFile = (filename, bytesBase64) => {
    try {
        const link = document.createElement('a');
        link.download = filename;
        link.href = "data:application/pdf;base64," + bytesBase64;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } 
    catch (err) {
        console.error("saveFile ERROR:", err);
    }
};

window.openPdfInNewTab = (bytesBase64) => {
    try {
        const byteCharacters = atob(bytesBase64);
        const byteNumbers = new Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: "application/pdf" });

        const url = URL.createObjectURL(blob);
        window.open(url, "_blank");

        // Release memory later
        setTimeout(() => URL.revokeObjectURL(url), 5000);
    } 
    catch (err) {
        console.error("openPdfInNewTab ERROR:", err);
    }
};
