

// drag and drop file upload
// This script handles drag-and-drop file uploads and displays the file name and thumbnail preview.


    const dropZone = document.getElementById('drop-zone');
    const fileInput = document.getElementById('fileInput');
    const fileNameDisplay = document.getElementById('file-name-display');
    const dropMessage = document.getElementById('drop-message');
    const thumbnail = document.getElementById('thumbnail-preview');

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('dragover');
    });

    dropZone.addEventListener('dragleave', () => {
        dropZone.classList.remove('dragover');
    });

    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('dragover');

        if (e.dataTransfer.files.length) {
            fileInput.files = e.dataTransfer.files;
            handleUpload(fileInput.files[0]);
        }
    });

    fileInput.addEventListener('change', () => {
        if (fileInput.files.length) {
            handleUpload(fileInput.files[0]);
        }
    });

    function handleUpload(file) {
        fileNameDisplay.textContent = `✅ Uploaded: ${file.name}`;
        dropMessage.style.display = 'none';

        const reader = new FileReader();
        reader.onload = function (e) {
            thumbnail.src = e.target.result;
            thumbnail.style.display = 'block';
        };
        reader.readAsDataURL(file);
    }

