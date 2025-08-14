import katex from 'https://cdn.jsdelivr.net/npm/katex@0.16.22/dist/katex.mjs';

export function renderLatexOutput(latex, element) {
    katex.render(latex, element);
}