let midiAccess = null;
let output = null;
const instrumentName = ['go:keys', 'roland', 'bluetooth'];
let dotNetHelper = null;

export function setOnStateChangeCallback(dotnetRef) {
    dotNetHelper = dotnetRef;
}

export async function connectMIDI() {
    if (!navigator.requestMIDIAccess) {
        alert('Web MIDI API not supported in this browser.');
        return false;
    }

    midiAccess = await navigator.requestMIDIAccess();


    if (midiAccess) {
        midiAccess.onstatechange = (event) => {
            const eventPort = event?.port;
            if (eventPort) {
                console.log(eventPort.name, eventPort.manufacturer, eventPort.state);
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnMidiStateChanged', eventPort.state, eventPort.name || 'Unknown Device');
                }
                if (output && eventPort.id === output.id && eventPort.state === 'disconnected') {
                    output = null;
                    console.log('Active MIDI output was disconnected');
                }
            }
        };

        midiAccess.inputs.forEach((input) => {
            console.log('Listening to MIDI input:', input.name);
            input.onmidimessage = (message) => {
                handleMIDIMessage(message);
            };
        });

        // Fonction commune pour gérer les interactions avec les touches
        const handlePianoKeyInteraction = (event: Event) => {
            const target = event.target as HTMLElement;
            if (target && target.classList.contains('piano-key')) {
                const note = target.dataset?.note;
                if (note && output) {
                    console.log(`Piano key ${event.type}:`, note);
                    const midiNote = noteNameToMidi(note, 4);
                    playNote(midiNote, 100, 1000);
                }
            }
        };

        // Écouter plusieurs types d'événements
        const eventTypes = ['touchstart', 'mousedown'];
        eventTypes.forEach(eventType => {
            document.addEventListener(eventType, handlePianoKeyInteraction);
        });

    }
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

export async function disconnectMIDI() {
    try {
        if (output) {
            await output.close();
            output = null;
            console.log('MIDI output disconnected');
        }
        if (midiAccess) {
            midiAccess.inputs.forEach((input) => {
                input.close();
            });
            midiAccess.outputs.forEach((output) => {
                output.close();
            });
            midiAccess.onstatechange = null;
            dotNetHelper = null;
            midiAccess = null;
            console.log('MIDI access disconnected');
        }
        return false;
    }
    catch (error) {
        console.error('Error disconnecting MIDI:', error);
        return true;
    }
}

export function sendProgramChange(channel, msb, lsb, pc) {
    if (!output)
        return;

    console.log(`PC to channel ${channel}: MSB=${msb}, LSB=${lsb}, PC=${pc}`);

    const ch = Math.max(1, Math.min(channel, 16)) - 1;
    const msbVal = Math.max(0, Math.min(msb, 127));
    const lsbVal = Math.max(0, Math.min(lsb, 127));
    const pcVal = Math.max(1, Math.min(pc, 128)) - 1;

    output.send([0xB0 | ch, 0x00, msbVal]);
    output.send([0xB0 | ch, 0x20, lsbVal]);
    output.send([0xC0 | ch, pcVal]);

    //Play sample :
    //0x90 + 3 pour le canal 4
    const notes = [48, 55, 60]; // DO SOL DO
    const velocity = 40; // Volume (velocity) between 0 and 127
    const duration = 40;

    //// Send "Note On" messages for all three notes
    //notes.forEach((note) => {
    //    const noteOnMessage = [0x93, note, velocity]; // Channel 1, note, velocity
    //    output.send(noteOnMessage);
    //});

    //// Stop the notes after 1 second with "Note Off" messages
    //setTimeout(() => {
    //    notes.forEach((note) => {
    //        const noteOffMessage = [0x83, note, 0]; // Channel 1, note, velocity 0
    //        output.send(noteOffMessage);
    //    });
    //}, duration);

    const playNote = (note, delay) => {
        setTimeout(() => {

            output.send([0x93, note, velocity]);

            setTimeout(() => {
                output.send([0x83, note, 0]);
            }, duration);
        }, delay);
    };

    // Jouer les notes successivement avec des délais
    notes.forEach((note, index) => {
        playNote(note, index * (duration + 100)); // Ajouter un délai entre les notes
    });
}

// Function to handle incoming MIDI messages
function handleMIDIMessage(message) {
    const [status, key, velocity] = message.data;

    // Check if the message is a "Note On" event (key pressed)
    if (status >= 144 && status < 160) {
        console.log(`Key pressed: ${key}, Velocity: ${velocity}`);

        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnMidiKeyPress', key, velocity || 'Unknown Device');
        }
    }

    // Check if the message is a "Note Off" event (key released)
    if (status >= 128 && status < 144) {
        console.log(`Key released: ${key}`);
    }
}

function playNote(note = 60, velocity = 127, duration = 500) {
    // Note on: [144, note, velocity]
    // 144 = 0x90, note, velocity
    output.send([0x93, note, velocity]); // 0x90 = Note On, channel 1
    // Note off after `duration` milliseconds
    setTimeout(() => {
        output.send([0x83, note, 0]); // 0x80 = Note Off, channel 1
    }, duration);
}

// Fonction pour convertir le nom de note en numéro MIDI
function noteNameToMidi(noteName: string, octave: number = 4): number {
    const noteMap: { [key: string]: number } = {
        'C': 0,
        'C#': 1,
        'D': 2,
        'D#': 3,
        'E': 4,
        'F': 5,
        'F#': 6,
        'G': 7,
        'G#': 8,
        'A': 9,
        'A#': 10,
        'B': 11
    };

    const noteValue = noteMap[noteName.toUpperCase()];
    if (noteValue === undefined) {
        console.warn(`Note inconnue: ${noteName}`);
        return 60; // C4 par défaut
    }

    return (octave + 1) * 12 + noteValue;
}