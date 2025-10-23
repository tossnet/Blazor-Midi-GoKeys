let midiAccess = null;
let output = null;
const instrumentName = ['go:keys', 'roland', 'bluetooth'];
export async function connectMIDI() {
    if (!navigator.requestMIDIAccess) {
        alert('Web MIDI API not supported in this browser.');
        return false;
    }
    navigator.requestMIDIAccess().then((access) => {
        access.inputs.forEach((input) => {
            console.log('input', input.name);
        });
        access.outputs.forEach((output) => {
            console.log('output', output.name);
        });
        access.onstatechange = (event) => {
            if (event.port) {
                console.log(event.port.name, event.port.manufacturer, event.port.state);
            }
        };
    });
    midiAccess = await navigator.requestMIDIAccess();
    midiAccess.outputs.forEach((out) => {
        console.log('output', out && out.name);
        const outName = out?.name;
        if (outName && instrumentName.some(name => outName.toLowerCase().includes(name))) {
            output = out;
        }
    });
    if (!output) {
        alert(`Not found any of: ${instrumentName.join(', ')}.`);
        return false;
    }
    return true;
}
//# sourceMappingURL=Home.razor.js.map