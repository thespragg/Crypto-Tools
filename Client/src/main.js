import { createApp } from 'vue'
import App from './App.vue'
import './index.css'
import axios from 'axios'

const http = axios.create({
    baseURL: "https://backtestapi.spraggtalksmoney.com/api/"
})
createApp(App).provide('$http', http).mount('#app')
