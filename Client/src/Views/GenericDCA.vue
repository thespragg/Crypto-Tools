<template>
  <div class="flex justify-center flex-col p-6">
    <div
      class="w-full rounded bg-gray-200 flex justify-center flex-col items-center"
    >
      <p class="p-6 text-lg">
        This strategy simulates what dollar cost averaging into the top coins by
        market cap over time would be.
      </p>
      <div class="h-px w-10/12 bg-gray-800" />
      <p class="text-red-700 font-bold mt-6" v-if="err">{{ err }}</p>
      <div class="flex flex-col sm:hidden p-6 w-full text-left justify-center">
        <div class="w-full flex items-center justify-center">
          <p class="w-1/2">DCA Amount</p>
          <n-input-number
            v-model:value="amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">DCA Interval</p>
          <n-select
            v-model:value="interval"
            :options="options"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">Start Date</p>
          <n-date-picker
            v-model:value="start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
        <div class="w-full flex items-center">
          <p class="w-1/2">End Date</p>
          <n-date-picker
            v-model:value="end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/2"
          />
        </div>
      </div>
      <div class="p-6 font-bold flex-col w-full items-center justify-center hidden sm:flex">
        <div class="flex w-full mb-2 items-center justify-center">
          <p class="w-1/5">DCA Amount</p>
          <p class="w-1/5">DCA Interval</p>
          <p class="w-1/5">Start Date</p>
          <p class="w-1/5">End Date</p>
        </div>
        <n-input-group class="justify-center">
          <n-input-number
            v-model:value="amount"
            placeholder="amount"
            :min="1"
            :max="5000"
            class="w-1/5"
          />
          <n-select
            v-model:value="interval"
            :options="options"
            class="w-1/5"
          />
          <n-date-picker
            v-model:value="start"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
          <n-date-picker
            v-model:value="end"
            type="date"
            clearable
            :is-date-disabled="dateDisabled"
            class="w-1/5"
          />
        </n-input-group>
        <n-button type="info" class="bg-blue-600 ml-2 mt-4 w-48" @click="run">
          Run
        </n-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { inject, ref } from "vue";
import {
  NInput,
  NInputNumber,
  NSelect,
  NDatePicker,
  NButton,
  NTable,
  NInputGroup,
  NSpin,
} from "naive-ui";

import { Chart, registerables } from "chart.js";
Chart.register(...registerables);
import { LineChart } from "vue-chart-3";
const http = inject("$http");

const coins = ref([])
const amount = ref(100)
const interval = ref("monthly")
const start = ref(Date.now() - 86400000 * 7)
const end = ref(Date.now())
const err = ref(null);
const dateDisabled = (ts) => ts < new Date("2013-01-01");

const options = [
  {
    label: "Weekly",
    value: "weekly",
  },
  {
    label: "Monthly",
    value: "monthly",
  },
];

const run = () => {
    
}
</script>
