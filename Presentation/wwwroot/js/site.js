document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('darkModeToggle');
    if (!toggle) return;

    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
        toggle.checked = true;
    } else {
        document.documentElement.setAttribute('data-theme', 'light');
        toggle.checked = false;
    }

    toggle.addEventListener('change', function () {
        if (this.checked) {
            document.documentElement.setAttribute('data-theme', 'dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        }
    });
});


function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.classList.remove('input-validation-error');
    });

    form.querySelectorAll('[data-valmsg-for]').forEach(span => {
        span.innerText = '';
        span.classList.remove('field-validation-error');
    });
}

function capitalizeFirstLetter(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

async function loadImage(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();

        reader.onerror = () => reject(new Error("Failed to load file."));
        reader.onload = (e) => {
            const img = new Image();
            img.onerror = () => reject(new Error("Failed to load image."));
            img.onload = () => resolve(img);
            img.src = e.target.result;
        };

        reader.readAsDataURL(file);
    });
}

async function processImage(file, imagePreview, previewer, previewSize = 150) {
    try {
        const img = await loadImage(file);
        const canvas = document.createElement('canvas');
        canvas.width = previewSize;
        canvas.height = previewSize;

        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(img, 0, 0, previewSize, previewSize);
        imagePreview.src = canvas.toDataURL('image/png');
        previewer.classList.add('selected');
    }
    catch (error) {
        console.error('Failed on image-processing.', error);
    }
}

function populateForm(form, data) {
    for (const key in data) {
        if (data.hasOwnProperty(key)) {
            const value = data[key];

            if (value && typeof value === 'object' && !Array.isArray(value)) {
                if (value.id) {
                    const input = form.querySelector(`[name="${key}Id"]`);
                    if (input) input.value = value.id;
                }
            } else {
                let input = form.querySelector(`[name="${key}"]`);
                if (!input) {
                    input = form.querySelector(`[name="${capitalizeFirstLetter(key)}"]`);
                }
                if (input) {
                    if (input.type === 'date' && typeof value === 'string') {
                        input.value = value.split('T')[0];
                    } else {
                        input.value = value ?? '';
                    }
                }
            }
        }
    }

    form.querySelectorAll('.form-select').forEach(select => {
        const input = select.querySelector('input[type="hidden"]');
        const text = select.querySelector('.form-select-text');
        if (input && text) {
            const selectedOption = select.querySelector(`.form-select-option[data-value="${input.value}"]`);
            text.textContent = selectedOption ? selectedOption.textContent : (select.dataset.placeholder || "Choose");
            select.classList.toggle('has-placeholder', !input.value);
        }
    });

    const imagePreview = form.querySelector('.image-preview');
    if (imagePreview && data.image) {
        const img = new Image();
        img.crossOrigin = "anonymous";
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = 150;
            canvas.height = 150;
            const ctx = canvas.getContext('2d');
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, 150, 150);
            imagePreview.src = canvas.toDataURL('image/png');

            const previewer = imagePreview.closest('.image-previewer');
            if (previewer) {
                previewer.classList.add('selected');
            }
        };
        img.onerror = () => {
            console.error('Failed to load and process image from URL');
        };
        img.src = data.image;
    }
}

document.addEventListener('DOMContentLoaded', () => {

    document.querySelectorAll('.form-select').forEach(select => {
        const trigger = select.querySelector('.form-select-trigger');
        const triggerText = trigger.querySelector('.form-select-text');
        const options = select.querySelectorAll('.form-select-option');
        const hiddenInput = select.querySelector('input[type="hidden"]');
        const placeholder = select.dataset.placeholder || "Choose";

        const setValue = (value = "", text = placeholder) => {
            triggerText.textContent = text;
            hiddenInput.value = value;
            select.classList.toggle('has-placeholder', !value);
        };

        setValue();

        trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            document.querySelectorAll('.form-select.open').forEach(el => el !== select && el.classList.remove('open'));
            select.classList.toggle('open');
        });

        options.forEach(option =>
            option.addEventListener('click', () => {
                setValue(option.dataset.value, option.textContent);
                select.classList.remove('open');
            })
        );

        document.addEventListener('click', e => {
            document.querySelectorAll('.form-select.open').forEach(select => {
                if (!select.contains(e.target)) {
                    select.classList.remove('open');
                }
            });
        });
    });


    const dropdowns = document.querySelectorAll('[data-type="dropdown"]');
    document.addEventListener('click', function (event) {
        let clickedDropdown = null;

        dropdowns.forEach(dropdown => {
            const targetId = dropdown.getAttribute('data-target');
            const targetElement = document.querySelector(targetId);

            if (dropdown.contains(event.target)) {
                clickedDropdown = targetElement;

                document.querySelectorAll('.dropdown.dropdown-show').forEach(openDropdown => {
                    if (openDropdown !== targetElement) {
                        openDropdown.classList.remove('dropdown-show');
                    }
                });

                targetElement.classList.toggle('dropdown-show');
            }
        });

        if (!clickedDropdown && !event.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown.dropdown-show').forEach(openDropdown => {
                openDropdown.classList.remove('dropdown-show');
            });
        }
    });


    const modalButtons = document.querySelectorAll('[data-modal="true"]');
    modalButtons.forEach(button => {
        button.addEventListener('click', async () => {
            const modalTarget = button.getAttribute('data-target');
            const modal = document.querySelector(modalTarget);
            const projectId = button.getAttribute('data-project-id');
            const userId = button.getAttribute('data-user-id')
            const clientId = button.getAttribute('data-client-id')

            if (modal) {
                const form = modal.querySelector('form');
                if (projectId) {
                    try {
                        const res = await fetch(`/projects/get?id=${projectId}`);
                        const project = await res.json();
                        if (form) {
                            populateForm(form, project);
                        }
                    } catch (err) {
                        console.error("Failed to load project:", err);
                    }
                }
                else if (userId) {
                    const res = await fetch(`/members/get?id=${userId}`);
                    const user = await res.json();
                    if (form) {
                        populateForm(form, user);
                    }
                }
                else if (clientId) {
                    const res = await fetch(`/clients/get?id=${clientId}`);
                    const user = await res.json();
                    if (form) {
                        populateForm(form, user);
                    }
                }
                modal.style.display = 'grid';
            }
        });
    });

  
    const previewSize = 150;
    const closeButtons = document.querySelectorAll('[data-close="true"]');
    closeButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('.modal');
            if (modal) {
                modal.style.display = 'none';

                modal.querySelectorAll('form').forEach(form => {
                    form.reset();

                    const imagePreview = form.querySelector('.image-preview');
                    if (imagePreview)
                        imagePreview.src = '';

                    const imagePreviewer = form.querySelector('.image-previewer');
                    if (imagePreviewer)
                        imagePreviewer.classList.remove('selected');
                });
            }
        });
    });

 
    document.querySelectorAll('.image-previewer').forEach(previewer => {
        const fileInput = previewer.querySelector('input[type="file"]');
        const imagePreview = previewer.querySelector('.image-preview');

        previewer.addEventListener('click', () => fileInput.click());

        fileInput.addEventListener('change', ({ target: { files } }) => {
            const file = files[0];
            if (file)
                processImage(file, imagePreview, previewer, previewSize);
        });
    });

   
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            clearErrorMessages(form);
            const formData = new FormData(form);

            try {
                const res = await fetch(form.action, {
                    method: 'post',
                    body: formData
                });

                if (res.ok) {
                    const modal = form.closest('.modal');
                    if (modal)
                        modal.style.display = 'none';

                    window.location.reload();
                }
                else if (res.status === 400) {
                    const data = await res.json();

                    if (data.errors) {
                        Object.keys(data.errors).forEach(key => {
                            let input = form.querySelector(`[name="${key}"]`);
                            if (input) {
                                input.classList.add('input-validation-error');
                            }

                            let span = form.querySelector(`[data-valmsg-for="${key}"]`);
                            if (span) {
                                span.innerText = data.errors[key].join('\n');
                                span.classList.add('field-validation-error');
                            }
                        });
                    }
                }
            }
            catch {
                console.log('Error submitting the form.');
            }
        });
    });

});
