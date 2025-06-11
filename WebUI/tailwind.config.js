/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './**/*.razor',
        './wwwroot/index.html'
    ],
    theme: {
        extend: {
            fontFamily: {
                sans: ['Inter', 'sans-serif'],
            },
        },
    },
    plugins: [],
}