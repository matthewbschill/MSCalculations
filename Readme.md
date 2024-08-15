# Title
Cost of Borrowing

# Description
See/maintain history of interest rates.
Calculate how much it will cost you overall to take out a loan for a particular number of months at a particular rate.

# Reason
Thinking about taking out a loan or refinancing an existing loan?
Ever wonder how much a particular loan will cost you in the long run?
What are historical best interest rates / what is the current trend?


# Technologies

Small REST API utilizing:
   .Net 8 (Core)
   Docker Containers
   Swagger (Swashbuckle.AspNetCore) --- Adds Docmunetation as well as a quick interface for testing
   SQL Lite and EFCore for a simple dev database
   FluentValidation - for some basic input validation

The idea would be that a dev would use the local db for initial testing 
and replace it with an actual MS SQL Server db for production 
(which may/may not already exist - thus you may/may not want to use EFCore to stand it up).

# Building

Image can be built with the following command:
docker build -t mscalculations .

# Running

Image can be run from docker desktop.
Be sure to specify a host port that maps to 8080.
For example: 5000: 8080

To run the container via command line, command would be similar to:
docker run -d -p 5000:8080 --name mscalculations mscalculations

Can stop the container via docker desktop or via command:
docker stop mscalculations

# Testing

Can test submitting REST calls in swagger.
For example if container is running on port 5000,
can open:
'http://localhost:5000/swagger/index.html'

in your browser.
