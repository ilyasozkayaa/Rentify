import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import vue from "eslint-plugin-vue";

export default tseslint.config(
  { ignores: ["dist/**", "node_modules/**", "bin/**", "obj/**", "wwwroot/**"] },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  ...vue.configs["flat/recommended"],
  {
    files: ["**/*.{ts,vue}"],
    languageOptions: { globals: { ...globals.browser } },
    rules: {
      "vue/multi-word-component-names": "off",
      "vue/valid-template-root": "off",
      "vue/max-attributes-per-line": "off",
      "vue/singleline-html-element-content-newline": "off",
      "vue/multiline-html-element-content-newline": "off",
      "vue/html-indent": "off",
      "vue/html-closing-bracket-newline": "off",
      "vue/html-self-closing": "off",
    },
  },
  {
    files: ["**/*.vue"],
    languageOptions: { parserOptions: { parser: tseslint.parser } },
  },
);
