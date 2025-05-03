function initQuills() {
    document.querySelectorAll('[data-quill-editor]').forEach(editor => {
        const editorId = editor.id;
        const textarea = document.querySelector(`[data-quill-textarea="#${editorId}"]`);
        const toolbarId = editor.getAttribute('data-quill-toolbar');

        const quill = new Quill(`#${editorId}`, {
            modules: {
                syntax: true,
                toolbar: `#${toolbarId}`
            },
            theme: 'snow',
            placeholder: 'Type something...'
        });

        if(textarea) {
            quill.on('text-change', () => {
                textarea.value = quill.root.innerHTML;
            });
        }
    });
}

function initModals(){
    document.querySelectorAll('[data-type="modal"]').forEach(trigger => {
        const target = document.querySelector(trigger.getAttribute('data-target'))
        trigger.addEventListener('click', () => {
            target?.classList.add('modal-show')
        })
        document.querySelectorAll('[data-type="close"]').forEach(btn => {
            const target = document.querySelector(btn.getAttribute('data-target'))
            btn.addEventListener('click', () => {
                target?.classList.remove('modal-show')
            })
        })
    })
}

function initDropdowns() {
    document.addEventListener('click', (e) => {
        let clickedInsideDropdown = false

        document.querySelectorAll('[data-type="dropdown"]').forEach(dropdownTrigger => {
            const targetId = dropdownTrigger.getAttribute('data-target')
            const dropdown = document.querySelector(targetId)

            if (dropdownTrigger.contains(e.target)) {
                clickedInsideDropdown = true
                document.querySelectorAll('.dropdown.dropdown-show').forEach(open => {
                    if (open !== dropdown) open.classList.remove('dropdown-show')
                })
                dropdown?.classList.toggle('dropdown-show')
            }
        })
        if (!clickedInsideDropdown && !e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown.dropdown-show').forEach(open => {
                open.classList.remove('dropdown-show')
            })
        }
    })
}

function initFileUploads() {
    document.querySelectorAll('[data-file-upload]').forEach(container => {
        const input = container.querySelector('input[type="file"]');
        const preview = container.querySelector('img');
        const iconContainer = container.querySelector('.circle');
        const icon = iconContainer?.querySelector('i');

        container.addEventListener('click', () => {
            input?.click();
        });

        input?.addEventListener('change', e => {
            const file = e.target.files[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = () => {
                    preview.src = reader.result;
                    preview.classList.remove('hide');
                    iconContainer.classList.add('selected');
                    icon.classList.remove('fa-camera');
                    icon.classList.add('fa-pen-to-square');
                };
                reader.readAsDataURL(file);
            }
        });
    });
}

function initCustomSelects() {
    document.querySelectorAll('.form-select').forEach(select => {
        const trigger = select.querySelector('.form-select-trigger');
        const triggerText = trigger.querySelector('.form-select-text');
        const options = select.querySelectorAll('.form-select-option');
        const hiddenInput = select.querySelector('input[type="hidden"]');
        const placeholder = select.dataset.placeholder || 'Select an option';

        const setValue = (value = "", text = placeholder) => {
            triggerText.textContent = text;
            hiddenInput.value = value;
            select.classList.toggle('has-placeholder', !value);
        };

        setValue();

        trigger.addEventListener('click', e => {
            e.stopPropagation();
            document.querySelectorAll('.form-select.open').forEach(el => {
                if (el !== select) el.classList.remove('open');
            });
            select.classList.toggle('open');
        });

        options.forEach(option => {
            option.addEventListener('click', () => {
                setValue(option.dataset.value, option.textContent);
                select.classList.remove('open');
            });
        });

        document.addEventListener('click', (e) => {
            if (!select.contains(e.target)) select.classList.remove('open');
        });
    });
}

function initializeApp() {
    initQuills();
    initModals();
    initDropdowns();
    initFileUploads();
    initCustomSelects();
}

document.addEventListener('DOMContentLoaded', initializeApp);
