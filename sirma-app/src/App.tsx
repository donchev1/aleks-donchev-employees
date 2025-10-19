import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import FileUploadForm from './Components/FileUploadForm'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
     <h1>Calculate Employees that have worked together for the longest time</h1>
     <div>
      <h2>Conditions</h2>
      <div className='info'>
        <div>1. Please upload a csv file in the following format: "EmpID, ProjectID, DateFrom, DateTo"</div>
        <div>2. If you have a NULL value in one of the date fields it will be considered as 'Today'</div>
        <div>3. You can specify your own date format in the form. If you don't specify anything the default format [yyyy-MM-dd] will be used.</div>
      </div>
        <FileUploadForm></FileUploadForm>
        
     </div>
    </>
  )
}

export default App
