async function gkClick_init() {
    const ctx = new ((window as any).AudioContext || (window as any).webkitAudioContext)();
    let sampleBuffer = null;
    const gainNode = ctx.createGain();
    gainNode.gain.value = 0.02;
    gainNode.connect(ctx.destination);
    try {
        const res = await fetch('touch.wav');
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.arrayBuffer();
        sampleBuffer = await ctx.decodeAudioData(data);
    } catch (err) {
        console.error('gkClick_init: failed to load sound', err);
        return;
    }
    document.querySelectorAll('.gk-button').forEach(btn => {
        btn.addEventListener('click', () => {
            if (!sampleBuffer) return;
            const source = ctx.createBufferSource();
            source.buffer = sampleBuffer;
            source.connect(gainNode);
            source.start(0);
        });
    });
};