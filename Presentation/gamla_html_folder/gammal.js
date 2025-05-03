const addProjectDescriptionTextarea = document.getElementById('add-project-description')
const addProjectDescriptionQuill = new Quill('#add-project-description-wysiwyg-editor',  {
    modules: {
        syntax: true,
        toolbar: '#add-project-description-wysiwyg-toolbar'
    },
    theme: 'snow',
    placeholder: 'Type something...'
});

addProjectDescriptionQuill.on('text-change', function(){
    addProjectDescriptionTextarea.value = addProjectDescriptionQuill.root.innerHTML
})

const editProjectDescriptionTextarea = document.getElementById('edit-project-description')
const editProjectDescriptionQuill = new Quill('#edit-project-description-wysiwyg-editor',  {
    modules: {
        syntax: true,
        toolbar: '#edit-project-description-wysiwyg-toolbar'
    },
    theme: 'snow',
    placeholder: 'Type something...'
});

editProjectDescriptionQuill.on('text-change', function(){
    editProjectDescriptionTextarea.value = editProjectDescriptionQuill.root.innerHTML
})

const uploadTrigger = document.getElementById('upload-trigger')
const fileInput = document.getElementById('image-upload')
const imagePreview = document.getElementById('image-preview')
const imagePreviewIcon = document.getElementById('image-preview-icon')
const imagePreviewIconContainer = document.getElementById('image-preview-icon-container')

uploadTrigger.addEventListener('click', function(){
    fileInput.click()
})

fileInput.addEventListener('change', function(e){
    const file = e.target.files[0]
    if(file && file.type.startsWith('image/')){
        const reader = new FileReader()
        reader.onload = (e) => {
            imagePreview.src = e.target.result
            imagePreview.classList.remove('hide')
            imagePreviewIcon.classList.remove('fa-camera')
            imagePreviewIconContainer.classList.add('selected')
            imagePreviewIcon.classList.add('fa-pen-to-square')
        }
        reader.readAsDataURL(file)
    }
})


const dropdowns = document.querySelectorAll('[data-type="dropdown"]')

document.addEventListener('click', function(event) {
    let clickedDropdown = null

    dropdowns.forEach(dropdown => {
        const targetId =  dropdown.getAttribute('data-target')
        const targetElement = document.querySelector(targetId)

        if (dropdown.contains(event.target)) {
            clickedDropdown = targetElement

            document.querySelectorAll('.dropdown.dropdown-show').forEach(openDropdown => {
                if (openDropdown !== targetElement) {
                    openDropdown.classList.remove('dropdown-show')
                }
            })

            targetElement.classList.toggle('dropdown-show')
        }

    })

    if (!clickedDropdown && !event.target.closest('.dropdown')) {
        document.querySelectorAll('.dropdown.dropdown-show').forEach(openDropdown => {
            openDropdown.classList.remove('dropdown-show')
        })
    }
})


const modals = document.querySelectorAll('[data-type="modal"]')
modals.forEach(modal => {
    modal.addEventListener('click', function(){
        const targetId = modal.getAttribute('data-target')
        const targetElement = document.querySelector(targetId)

        targetElement.classList.add('modal-show')
    })
})


const closeButtons = document.querySelectorAll('[data-type="close"]')
closeButtons.forEach(button => {
    button.addEventListener('click', function(){
        const targetId = button.getAttribute('data-target')
        const targetElement = document.querySelector(targetId)

        targetElement.classList.remove('modal-show')
    })
})


document.querySelectorAll('.form-select').forEach(select => {
    const trigger = select.querySelector('.form-select-trigger')
    const triggerText = trigger.querySelector('.form-select-text')
    const options = select.querySelectorAll('.form-select-options')
    const hiddenInput = select.querySelector('input[type="hidden"]')
    const placeholder = select.dataset.placeholder || "Choose an option"

    const setValue = (value = "", text = placeholder)  => {
        triggerText.textContent = text
        hiddenInput.value = value
        select.classList.toggle('has-placeholder', !value)
    };

    setValue()

    trigger.addEventListener('click', (e) => {
        e.stopPropagation();
        document.querySelectorAll('.form-select.open')
        .forEach(el => el !== select && el.classList.remove('open'))
        select.classList.toggle('open')
    })

    options.forEach(option => 
        option.addEventListener('click', () => {
            setValue(option.dataset.value, option.textContent)
            select.classList.remove('open')
        })
    )

    document.addEventListener('click', e => { 
        if(!select.contains(e.target))
            select.classList.remove('open')
    })
})
