let midiAccess: MIDIAccess | null = null;
let output: MIDIOutput | null = null;
const instrumentName: string[] = ['go:keys', 'roland', 'bluetooth']; // the second 2 are for Web MIDI browser on iPad

let dotNetHelper: any = null;

export function setOnStateChangeCallback(dotnetRef: any): void {
    dotNetHelper = dotnetRef;
}

export async function connectMIDI(): Promise<boolean> {
    if (!navigator.requestMIDIAccess) {
        alert('Web MIDI API not supported in this browser.');
        return false;
    }

    midiAccess = await navigator.requestMIDIAccess();

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

    // Add state change listener
    if (midiAccess) {
        midiAccess.onstatechange = (event: MIDIConnectionEvent) => {
            const eventPort = event?.port;
            if (eventPort) {
                console.log(eventPort.name, eventPort.manufacturer, eventPort.state);

                // Notify Blazor component
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnMidiStateChanged', eventPort.state, eventPort.name || 'Unknown Device');
                }

                // If the currently used port is disconnected
                if (output && eventPort.id === output.id && eventPort.state === 'disconnected') {
                    output = null;
                    console.log('Active MIDI output was disconnected');
                }
            }
        };
    }

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

export async function disconnectMIDI(): Promise<boolean> {
    try {
        if (output) {
            await output.close();
            output = null;
            console.log('MIDI output disconnected');
        }

        if (midiAccess) {
            // Optional : close all input ports
            midiAccess.inputs.forEach((input: MIDIInput) => {
                input.close();
            });

            // Optional : close all ouput ports
            midiAccess.outputs.forEach((output: MIDIOutput) => {
                output.close();
            });

            // remove listener
            midiAccess.onstatechange = null;
            dotNetHelper = null;

            midiAccess = null;
            console.log('MIDI access disconnected');
        }

        return false;
    } catch (error) {
        console.error('Error disconnecting MIDI:', error);
        return true;
    }
}

export function sendProgramChange(channel: number,
                                  msb: number,
                                  lsb: number,
                                  pc: number): void {

    if (!output) return;

    console.log(`PC to channel ${channel}: MSB=${msb}, LSB=${lsb}, PC=${pc}`);

    // Assure que les valeurs sont dans les plages MIDI valides
    const ch = Math.max(1, Math.min(channel, 16)) - 1;
    const msbVal = Math.max(0, Math.min(msb, 127));
    const lsbVal = Math.max(0, Math.min(lsb, 127));
    const pcVal = Math.max(1, Math.min(pc, 128)) - 1;

    output.send([0xB0 | ch, 0x00, msbVal]); // CC#0 MSB
    output.send([0xB0 | ch, 0x20, lsbVal]); // CC#32 LSB
    output.send([0xC0 | ch, pcVal]);        // Program Change (PC is 1-based)
}
