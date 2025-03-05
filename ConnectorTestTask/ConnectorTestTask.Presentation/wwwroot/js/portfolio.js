document.getElementById("portfolio-form").addEventListener("submit", function(event) {
    event.preventDefault();
    let formData = new FormData(this);

    fetch("/Portfolio/Calculate", {
        method: "POST",
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            let errorContainer = document.getElementById("error-container");
            if (!data.success) {
                errorContainer.innerText = data.message;
                errorContainer.style.display = "block";
                return;
            }
            errorContainer.style.display = "none";

            let table = document.getElementById("result-table");
            let tbody = table.querySelector("tbody");
            tbody.innerHTML = "";
            document.getElementById("target-label").innerText = data.targetCurrency;

            Object.entries(data.data).forEach(([currency, value]) => {
                let row = `<tr><td>${currency}</td><td>${value.toFixed(4)}</td></tr>`;
                tbody.innerHTML += row;
            });

            table.style.display = "block";
        })
        .catch(error => console.error("Ошибка:", error));
});

document.getElementById("add-currency").addEventListener("click", function() {
    let container = document.getElementById("currency-container");
    let index = container.childElementCount;
    let newRow = document.createElement("div");
    newRow.className = "currency-row";
    newRow.innerHTML = `
        <select name="Portfolio[${index}].Currency" class="currency-select">
            ${document.getElementById("target-currency").innerHTML}
        </select>
        <input type="number" name="Portfolio[${index}].Amount" step="0.0001" class="currency-amount" placeholder="Сумма" />
        <button type="button" onclick="removeCurrency(this)">Удалить</button>
    `;
    container.appendChild(newRow);
});

function removeCurrency(button) {
    button.parentElement.remove();
}
