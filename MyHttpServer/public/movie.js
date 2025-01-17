function addToBookmarks(movieId) {
    fetch(`/add-to-bookmarks?movieId=${movieId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ movieId: movieId })
    })
        .then(response => response.text())
        .then(html => {
            document.getElementById('response-container').innerHTML = html; // Или любая другая логика для отображения сообщения
        })
        .catch(error => console.error('Error:', error));
}