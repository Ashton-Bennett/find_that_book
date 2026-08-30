## Getting a Gemini API Key

1. **Visit Google AI Studio**  
   Go to the official [Google AI Studio](https://aistudio.google.com/) platform.

2. **Log In**  
   Sign in using your standard Google Account.

3. **Accept the Terms**  
   Read and accept the terms of service if prompted.

4. **Create an API Key**  
   Click **"Get API key"**, usually located in the top-left corner of the dashboard.

5. **Select a Project**  
   Click **"Create API key"**. You can attach the key to an existing Google Cloud project or automatically create a new one.

6. **Copy and Save**  
   Copy your generated API key and store it securely. Do not commit it to GitHub or include it directly in your source code.

## Setting Your Gemini API Key

1. **Open your terminal** and navigate to the project directory.

2. **Run the following commands:**
   cd server/Api
   dotnet user-secrets init
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_ACTUAL_API_KEY"
