import { createApp } from 'vue'
import App from './App.vue'
import './index.css'
import axios from 'axios'

const http = axios.create({
    baseURL: "https://localhost:7166/api/"
})
createApp(App).provide('$http', http).mount('#app')
