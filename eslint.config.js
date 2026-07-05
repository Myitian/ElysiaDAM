import js from "@eslint/js";
import globals from "globals";
import { defineConfig } from "eslint/config";

export default defineConfig([
  {
    files: ["Myitian.ElysiaDAM.WebFrontend/**/*"],
    plugins: { js },
    extends: ["js/recommended"],
    languageOptions: {
      globals: {
        ...globals.browser
      },
      sourceType: "script",
    },
    rules: {
      quotes: ["error", "double"],
      semi: ["error", "always"],
      "no-unused-vars": ["warn"],
    },
  },
  {
    files: ["Myitian.ElysiaDAM.WebFrontend/**/*.mjs"],
    languageOptions: {
      sourceType: "module",
    }
  },
]);