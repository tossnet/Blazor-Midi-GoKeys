let midiAccess: MIDIAccess | null = null;
let output: MIDIOutput | null = null;
const instrumentName: string[] = ['go:keys', 'roland', 'bluetooth']; // the second 2 are for Web MIDI browser on iPad

export async function connectMIDI(): Promise<boolean> {
    if (!navigator.requestMIDIAccess) {
        alert('Web MIDI API not supported in this browser.');
        return false;
    }

    // ** debug:
    navigator.requestMIDIAccess().then((access: MIDIAccess) => {
        // Get lists of available MIDI controllers using forEach instead of values()
        access.inputs.forEach((input) => {
            console.log('input', input.name);
        });
        access.outputs.forEach((output) => {
            console.log('output', output.name);
        });
        access.onstatechange = (event: MIDIConnectionEvent) => {
            // Print information about the (dis)connected MIDI controller
            if (event.port) {
                console.log(event.port.name, event.port.manufacturer, event.port.state);
            }
        };
    });
    // ** 

    midiAccess = await navigator.requestMIDIAccess();

    // Try to find Roland output port
    midiAccess.outputs.forEach((out: MIDIOutput) => {
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