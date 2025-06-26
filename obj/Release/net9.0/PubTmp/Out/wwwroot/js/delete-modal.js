document.addEventListener('DOMContentLoaded', () => {
    const modalElement = document.getElementById('confirmDeleteModal');
    if (!modalElement) return;

    modalElement.addEventListener('show.bs.modal', event => {
        const button = event.relatedTarget;          // botão que acionou o modal
        const title = button.dataset.title;
        const message = button.dataset.message;
        const url = button.dataset.url;

        modalElement.querySelector('.modal-title').textContent = title;
        modalElement.querySelector('.modal-body p').textContent = message;
        modalElement.querySelector('#deleteForm').action = url;
    });
});
