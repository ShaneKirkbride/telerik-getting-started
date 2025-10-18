window.configSetup = window.configSetup || {};

window.configSetup.triggerFilePicker = (element) => {
    if (!element) {
        return;
    }

    element.click();
};

window.configSetup.downloadFile = (fileName, content) => {
    if (!fileName || !content) {
        return;
    }

    const blob = new Blob([content], { type: 'application/xml;charset=utf-8' });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.style.display = 'none';
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);

    URL.revokeObjectURL(url);
};
