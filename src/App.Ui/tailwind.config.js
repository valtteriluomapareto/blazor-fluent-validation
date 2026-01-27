/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./**/*.{razor,html,cs}",
        "../App.Ui.Client/**/*.{razor,html,cs}",
        "../App.Host/**/*.{razor,html,cs}",
    ],
    theme: {
        extend: {},
    },
    plugins: [],
};
