function openParticipantPage() {
    const newWindow = window.open('', 'Participant', 'width=300,height=200');
    newWindow.document.write(`
                    <h2>Enter Participant Name</h2>
                    <input type="text" id="participantName" placeholder="Name" />
                    <button onclick="window.opener.addParticipant(document.getElementById('participantName').value); window.close();">Apply</button>
                `);
}

function addParticipant(name) {
    if (!name) return;
    const participantsContainer = document.getElementById("participantsContainer");

    // Add participant name to list
    const participantElement = document.createElement('p');
    participantElement.textContent = name;
    participantsContainer.appendChild(participantElement);

    updateDiagram();
}

function updateDiagram() {
    const participantsContainer = document.getElementById("participantsContainer");
    const participants = participantsContainer.querySelectorAll('p');
    const diagramContainer = document.getElementById("diagramContainer");

    // Clear previous slices
    diagramContainer.innerHTML = '';

    // Add slices depending on the number of participants
    const totalParticipants = participants.length;
    if (totalParticipants === 0) return;

    const angle = 360 / totalParticipants;
    for (let i = 0; i < totalParticipants; i++) {
        const slice = document.createElement('div');
        slice.className = `slice participant-${(i % 3) + 1}`;
        slice.style.transform = `rotate(${i * angle}deg) skewY(${90 - angle / 2}deg)`;
        diagramContainer.appendChild(slice);
    }
}