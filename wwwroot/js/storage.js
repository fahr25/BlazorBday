// Browser localStorage helper for OrderDraft persistence
window.orderStorage = {
    /**
     * Save order draft to localStorage
     * @param {string} json - Serialized OrderDraft JSON
     */
    save: function(json) {
        try {
            localStorage.setItem('orderDraft', json);
        } catch (e) {
            console.error('Failed to save order draft to localStorage:', e);
        }
    },

    /**
     * Load order draft from localStorage
     * @returns {string|null} Serialized OrderDraft JSON or null if not found
     */
    load: function() {
        try {
            return localStorage.getItem('orderDraft');
        } catch (e) {
            console.error('Failed to load order draft from localStorage:', e);
            return null;
        }
    },

    /**
     * Clear order draft from localStorage
     */
    clear: function() {
        try {
            localStorage.removeItem('orderDraft');
        } catch (e) {
            console.error('Failed to clear order draft from localStorage:', e);
        }
    }
};
