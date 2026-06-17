const TradingUI = {
    formatValue(value) {
        const numericValue = Number(value);
        return Number.isFinite(numericValue) ? numericValue.toFixed(4) : value;
    },

    updatePair(pairId, data) {
        const row = document.getElementById(`pair-${pairId}`);
        if (!row) {
            console.warn(`TradingUI: row not found for pair id ${pairId}.`);
            return;
        }

        const price = data.price ?? data.currentValue;
        const min = data.min ?? data.minValue;
        const max = data.max ?? data.maxValue;

        if (price !== undefined && price !== null) {
            const priceElement = row.querySelector('.price');
            if (priceElement) {
                priceElement.textContent = this.formatValue(price);
            }
        }

        if (min !== undefined && min !== null) {
            const minElement = row.querySelector('.min');
            if (minElement) {
                minElement.textContent = this.formatValue(min);
            }
        }

        if (max !== undefined && max !== null) {
            const maxElement = row.querySelector('.max');
            if (maxElement) {
                maxElement.textContent = this.formatValue(max);
            }
        }
    }
};

window.TradingUI = TradingUI;
