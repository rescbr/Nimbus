CKEDITOR.replace('txtTextMsg', {
    toolbar: [
        { name: 'document', items: ['Source', '-', 'NewPage', 'Preview', '-', 'Templates'] },	// Defines toolbar group with name (used to create voice label) and items in 3 subgroups.
        ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'],			// Defines toolbar group without name.
        '/',																					// Line break - next group will be placed in new line.
        { name: 'basicstyles', items: ['Bold', 'Italic'] }
    ]
});
